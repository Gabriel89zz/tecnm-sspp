using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TecNM.Api.Core.Entities;
using TecNM.Api.Core.Exceptions;
using TecNM.Api.Core.Http;
using TecNM.Api.Data;

namespace TecNM.Api.Modules.Companies;

public class AgreementsService(TecNMDbContext dbContext, IConfiguration configuration)
{
    private static readonly HashSet<string> ValidAgreementTypes =
    [
        "general_framework",
        "social_service",
        "internship",
        "dual_education",
        "collaboration"
    ];

    private static readonly byte[] PdfMagicBytes = [0x25, 0x50, 0x44, 0x46]; // %PDF

    private string StorageRootPath =>
        configuration["Storage:RootPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "storage");

    public async Task<PagedResponse<AgreementResponse>> GetPagedAsync(
        int page,
        int pageSize,
        long? companyId,
        string? agreementType,
        string? validity,
        string? search,
        string? sort,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var query = dbContext.Agreements
            .Include(a => a.Company)
            .AsNoTracking();

        if (companyId.HasValue)
        {
            query = query.Where(a => a.CompanyId == companyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(agreementType))
        {
            var normalizedType = agreementType.Trim().ToLowerInvariant();
            query = query.Where(a => a.AgreementType == normalizedType);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(a =>
                EF.Functions.ILike(a.AgreementNumber, pattern) ||
                (a.Company != null && EF.Functions.ILike(a.Company.Name, pattern)) ||
                (a.Description != null && EF.Functions.ILike(a.Description, pattern)));
        }

        var list = await query.ToListAsync(cancellationToken);

        // Filtrado por validity en memoria ya que depende de cálculo dinámico respecto a today
        if (!string.IsNullOrWhiteSpace(validity))
        {
            var normalizedValidity = validity.Trim().ToLowerInvariant();
            list = list.Where(a => CalculateValidity(a, today) == normalizedValidity).ToList();
        }

        // Ordenamiento
        list = sort?.ToLowerInvariant() switch
        {
            "-expirationdate" => list.OrderByDescending(a => a.ExpirationDate).ToList(),
            "expirationdate" => list.OrderBy(a => a.ExpirationDate).ToList(),
            "signeddate" => list.OrderBy(a => a.SignedDate).ToList(),
            "-signeddate" => list.OrderByDescending(a => a.SignedDate).ToList(),
            "agreementnumber" => list.OrderBy(a => a.AgreementNumber).ToList(),
            "-agreementnumber" => list.OrderByDescending(a => a.AgreementNumber).ToList(),
            _ => list.OrderBy(a => a.ExpirationDate).ToList()
        };

        var totalItems = list.Count;
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        var currentPage = Math.Max(1, page);

        var pagedItems = list
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(a => ToResponse(a, today))
            .ToList();

        return new PagedResponse<AgreementResponse>(pagedItems, currentPage, pageSize, totalItems, totalPages);
    }

    public async Task<IReadOnlyList<OptionItem>> GetOptionsAsync(
        long? companyId,
        string? tramiteType,
        DateOnly? onDate,
        CancellationToken cancellationToken = default)
    {
        var targetDate = onDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var query = dbContext.Agreements
            .Include(a => a.Company)
            .AsNoTracking()
            .Where(a => a.IsActive && a.SignedDate <= targetDate && a.ExpirationDate >= targetDate);

        if (companyId.HasValue)
        {
            query = query.Where(a => a.CompanyId == companyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(tramiteType))
        {
            var normalizedTramite = tramiteType.Trim().ToLowerInvariant();
            if (normalizedTramite == "social_service")
            {
                query = query.Where(a => a.AgreementType == "general_framework" ||
                                         a.AgreementType == "social_service" ||
                                         a.AgreementType == "collaboration");
            }
            else if (normalizedTramite == "internship")
            {
                query = query.Where(a => a.AgreementType == "general_framework" ||
                                         a.AgreementType == "internship" ||
                                         a.AgreementType == "dual_education" ||
                                         a.AgreementType == "collaboration");
            }
        }

        var items = await query
            .OrderBy(a => a.AgreementNumber)
            .Select(a => new
            {
                a.Id,
                a.AgreementNumber,
                CompanyName = a.Company != null ? a.Company.Name : "Sin empresa",
                a.AgreementType,
                a.ExpirationDate
            })
            .ToListAsync(cancellationToken);

        return items
            .Select(a => new OptionItem(
                a.Id,
                $"{a.AgreementNumber} · {a.CompanyName}",
                new Dictionary<string, object?>
                {
                    ["agreementType"] = a.AgreementType,
                    ["expirationDate"] = a.ExpirationDate.ToString("yyyy-MM-dd")
                }))
            .ToList();
    }

    public async Task<AgreementResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var agreement = await dbContext.Agreements
            .Include(a => a.Company)
            .AsNoTracking()
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (agreement is null)
        {
            throw new NotFoundException("agreements.not_found", $"Convenio con id {id} no encontrado.");
        }

        return ToResponse(agreement, today);
    }

    public async Task<AgreementResponse> CreateAsync(AgreementRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateCompanyAsync(request.CompanyId, cancellationToken);
        ValidateRequest(request);

        var normalizedNumber = request.AgreementNumber.Trim().ToUpperInvariant();
        var exists = await dbContext.Agreements
            .AnyAsync(a => a.AgreementNumber == normalizedNumber, cancellationToken);

        if (exists)
        {
            throw new ConflictException("agreements.duplicated_number", $"Ya existe un convenio registrado con el número '{normalizedNumber}'.");
        }

        var agreement = new Agreement
        {
            CompanyId = request.CompanyId,
            AgreementNumber = normalizedNumber,
            AgreementType = request.AgreementType.Trim().ToLowerInvariant(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            SignedDate = request.SignedDate,
            ExpirationDate = request.ExpirationDate,
            IsActive = true
        };

        dbContext.Agreements.Add(agreement);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(agreement.Id, cancellationToken);
    }

    public async Task<AgreementResponse> UpdateAsync(long id, AgreementRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateCompanyAsync(request.CompanyId, cancellationToken);
        ValidateRequest(request);

        var agreement = await dbContext.Agreements
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (agreement is null)
        {
            throw new NotFoundException("agreements.not_found", $"Convenio con id {id} no encontrado.");
        }

        var normalizedNumber = request.AgreementNumber.Trim().ToUpperInvariant();
        var exists = await dbContext.Agreements
            .AnyAsync(a => a.Id != id && a.AgreementNumber == normalizedNumber, cancellationToken);

        if (exists)
        {
            throw new ConflictException("agreements.duplicated_number", $"Ya existe otro convenio registrado con el número '{normalizedNumber}'.");
        }

        agreement.CompanyId = request.CompanyId;
        agreement.AgreementNumber = normalizedNumber;
        agreement.AgreementType = request.AgreementType.Trim().ToLowerInvariant();
        agreement.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        agreement.SignedDate = request.SignedDate;
        agreement.ExpirationDate = request.ExpirationDate;

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(agreement.Id, cancellationToken);
    }

    public async Task<AgreementResponse> UpdateStatusAsync(long id, bool isActive, CancellationToken cancellationToken = default)
    {
        var agreement = await dbContext.Agreements
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (agreement is null)
        {
            throw new NotFoundException("agreements.not_found", $"Convenio con id {id} no encontrado.");
        }

        agreement.IsActive = isActive;
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(agreement.Id, cancellationToken);
    }

    public async Task<AgreementResponse> UploadDocumentAsync(long id, IFormFile file, CancellationToken cancellationToken = default)
    {
        var agreement = await dbContext.Agreements
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (agreement is null)
        {
            throw new NotFoundException("agreements.not_found", $"Convenio con id {id} no encontrado.");
        }

        if (file.Length == 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["file"] = ["El archivo proporcionado está vacío."]
            });
        }

        // 10 MB limit
        const long maxSizeBytes = 10 * 1024 * 1024;
        if (file.Length > maxSizeBytes)
        {
            throw new AppException(413, "agreements.file_too_large", "El archivo supera el tamaño máximo permitido de 10 MB.", "Carga de archivo excedida");
        }

        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException(415, "agreements.invalid_format", "El documento debe ser un archivo en formato PDF (.pdf).", "Formato no compatible");
        }

        // Validar magic bytes (%PDF)
        using (var stream = file.OpenReadStream())
        {
            var header = new byte[4];
            var bytesRead = await stream.ReadAsync(header.AsMemory(0, 4), cancellationToken);
            if (bytesRead < 4 || !header.SequenceEqual(PdfMagicBytes))
            {
                throw new AppException(415, "agreements.invalid_pdf", "El archivo no contiene un encabezado PDF válido.", "Formato no compatible");
            }
        }

        var dirPath = Path.Combine(StorageRootPath, "agreements");
        Directory.CreateDirectory(dirPath);

        // Eliminar archivo anterior si existe
        if (!string.IsNullOrEmpty(agreement.DocumentUrl))
        {
            var oldFullPath = Path.Combine(StorageRootPath, agreement.DocumentUrl.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(oldFullPath))
            {
                try { File.Delete(oldFullPath); } catch { /* ignore */ }
            }
        }

        var fileName = $"{id}_{Guid.NewGuid():N}.pdf";
        var relativePath = $"agreements/{fileName}";
        var fullPath = Path.Combine(dirPath, fileName);

        using (var targetStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        {
            await file.CopyToAsync(targetStream, cancellationToken);
        }

        agreement.DocumentUrl = relativePath;
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(agreement.Id, cancellationToken);
    }

    public async Task<(Stream stream, string fileName)> GetDocumentStreamAsync(long id, CancellationToken cancellationToken = default)
    {
        var agreement = await dbContext.Agreements
            .AsNoTracking()
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (agreement is null || string.IsNullOrEmpty(agreement.DocumentUrl))
        {
            throw new NotFoundException("agreements.document_not_found", $"El convenio con id {id} no cuenta con un documento digital cargado.");
        }

        var fullPath = Path.Combine(StorageRootPath, agreement.DocumentUrl.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
        {
            throw new NotFoundException("agreements.file_missing", "El archivo físico del convenio no fue encontrado en el servidor.");
        }

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var downloadName = $"Convenio_{agreement.AgreementNumber}.pdf";
        return (stream, downloadName);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var agreement = await dbContext.Agreements
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (agreement is null)
        {
            throw new NotFoundException("agreements.not_found", $"Convenio con id {id} no encontrado.");
        }

        dbContext.Agreements.Remove(agreement);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidateCompanyAsync(long companyId, CancellationToken cancellationToken)
    {
        var company = await dbContext.Companies
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == companyId, cancellationToken);

        if (company is null)
        {
            throw new NotFoundException("companies.not_found", $"Empresa con id {companyId} no encontrada.");
        }

        if (!company.IsActive)
        {
            throw new BusinessRuleException("companies.inactive", "No se puede vincular un convenio a una empresa inactiva.");
        }
    }

    private static void ValidateRequest(AgreementRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.AgreementNumber) || request.AgreementNumber.Trim().Length > 64)
        {
            errors["agreementNumber"] = ["El número o clave de convenio es obligatorio y debe tener entre 1 y 64 caracteres."];
        }

        var type = request.AgreementType?.Trim().ToLowerInvariant() ?? "";
        if (!ValidAgreementTypes.Contains(type))
        {
            errors["agreementType"] = [$"El tipo de convenio '{request.AgreementType}' no es válido. Opciones: {string.Join(", ", ValidAgreementTypes)}."];
        }

        if (request.ExpirationDate < request.SignedDate)
        {
            errors["expirationDate"] = ["La fecha de vigencia/expiración debe ser mayor o igual a la fecha de firma del convenio."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    private static string CalculateValidity(Agreement a, DateOnly today)
    {
        if (!a.IsActive)
        {
            return "inactive";
        }

        if (today > a.ExpirationDate)
        {
            return "expired";
        }

        var days = a.ExpirationDate.DayNumber - today.DayNumber;
        if (days <= 60)
        {
            return "expiring";
        }

        return "valid";
    }

    private static AgreementResponse ToResponse(Agreement a, DateOnly today)
    {
        var validity = CalculateValidity(a, today);
        var daysToExpiration = a.ExpirationDate.DayNumber - today.DayNumber;
        var hasDoc = !string.IsNullOrEmpty(a.DocumentUrl);
        var companyName = a.Company?.Name ?? "Sin empresa vinculada";

        return new AgreementResponse(
            a.Id,
            a.CompanyId,
            companyName,
            a.AgreementNumber,
            a.AgreementType,
            a.Description,
            a.SignedDate,
            a.ExpirationDate,
            a.IsActive,
            hasDoc,
            validity,
            daysToExpiration,
            a.CreatedAt,
            a.UpdatedAt);
    }
}
