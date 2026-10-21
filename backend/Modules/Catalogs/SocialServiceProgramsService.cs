using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TecNM.Api.Core.Entities;
using TecNM.Api.Core.Exceptions;
using TecNM.Api.Core.Http;
using TecNM.Api.Data;

namespace TecNM.Api.Modules.Catalogs;

public partial class SocialServiceProgramsService(TecNMDbContext dbContext)
{
    private static readonly HashSet<string> ValidCategories =
    [
        "community_development",
        "adult_education",
        "contingency_programs",
        "environmental_care",
        "health_support",
        "civic_sports_cultural",
        "special_government_programs",
        "professionalizing_service",
        "institutional_programs"
    ];

    [GeneratedRegex(@"^[A-Z0-9_-]+$")]
    private static partial Regex CodeRegex();

    public async Task<PagedResponse<SocialServiceProgramResponse>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? category,
        bool? isInternal,
        bool? isActive,
        string? sort,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.SocialServicePrograms.AsNoTracking();

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        if (isInternal.HasValue)
        {
            query = query.Where(p => p.IsInternal == isInternal.Value);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category == category.Trim().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.Code, pattern) || EF.Functions.ILike(p.Name, pattern));
        }

        query = sort?.ToLowerInvariant() switch
        {
            "-code" => query.OrderByDescending(p => p.Code),
            "name" => query.OrderBy(p => p.Name),
            "-name" => query.OrderByDescending(p => p.Name),
            "category" => query.OrderBy(p => p.Category),
            _ => query.OrderBy(p => p.Code)
        };

        var projected = query.Select(p => new SocialServiceProgramResponse(
            p.Id,
            p.Code,
            p.Name,
            p.Category,
            p.IsInternal,
            p.Description,
            p.IsActive,
            p.CreatedAt,
            p.UpdatedAt));

        return await projected.ToPagedResponseAsync(page, pageSize, cancellationToken);
    }

    public async Task<IReadOnlyList<OptionItem>> GetOptionsAsync(CancellationToken cancellationToken = default)
    {
        var items = await dbContext.SocialServicePrograms
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Code)
            .Select(p => new { p.Id, p.Code, p.Name, p.Category, p.IsInternal })
            .ToListAsync(cancellationToken);

        return items
            .Select(p => new OptionItem(
                p.Id,
                $"{p.Code} · {p.Name}",
                new Dictionary<string, object?>
                {
                    ["code"] = p.Code,
                    ["category"] = p.Category,
                    ["isInternal"] = p.IsInternal
                }))
            .ToList();
    }

    public async Task<SocialServiceProgramResponse> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var program = await dbContext.SocialServicePrograms
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (program is null)
        {
            throw new NotFoundException("social_service_programs.not_found", "Programa de servicio social no encontrado.");
        }

        return ToResponse(program);
    }

    public async Task<SocialServiceProgramResponse> CreateAsync(
        SocialServiceProgramRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var exists = await dbContext.SocialServicePrograms
            .AnyAsync(p => p.Code == normalizedCode, cancellationToken);

        if (exists)
        {
            throw new ConflictException("social_service_programs.duplicated_code", $"Ya existe un programa con la clave '{normalizedCode}'.");
        }

        var program = new SocialServiceProgram
        {
            Code = normalizedCode,
            Name = request.Name.Trim(),
            Category = request.Category.Trim().ToLowerInvariant(),
            IsInternal = request.IsInternal,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsActive = true
        };

        dbContext.SocialServicePrograms.Add(program);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(program);
    }

    public async Task<SocialServiceProgramResponse> UpdateAsync(
        long id,
        SocialServiceProgramRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var program = await dbContext.SocialServicePrograms
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (program is null)
        {
            throw new NotFoundException("social_service_programs.not_found", "Programa de servicio social no encontrado.");
        }

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var codeInUse = await dbContext.SocialServicePrograms
            .AnyAsync(p => p.Id != id && p.Code == normalizedCode, cancellationToken);

        if (codeInUse)
        {
            throw new ConflictException("social_service_programs.duplicated_code", $"Ya existe otro programa con la clave '{normalizedCode}'.");
        }

        program.Code = normalizedCode;
        program.Name = request.Name.Trim();
        program.Category = request.Category.Trim().ToLowerInvariant();
        program.IsInternal = request.IsInternal;
        program.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(program);
    }

    public async Task<SocialServiceProgramResponse> UpdateStatusAsync(
        long id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var program = await dbContext.SocialServicePrograms
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (program is null)
        {
            throw new NotFoundException("social_service_programs.not_found", "Programa de servicio social no encontrado.");
        }

        program.IsActive = isActive;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(program);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var program = await dbContext.SocialServicePrograms
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (program is null)
        {
            throw new NotFoundException("social_service_programs.not_found", "Programa de servicio social no encontrado.");
        }

        dbContext.SocialServicePrograms.Remove(program);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateRequest(SocialServiceProgramRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Code) || request.Code.Trim().Length > 32)
        {
            errors["code"] = ["La clave es obligatoria y debe tener entre 1 y 32 caracteres."];
        }
        else if (!CodeRegex().IsMatch(request.Code.Trim().ToUpperInvariant()))
        {
            errors["code"] = ["La clave solo puede contener letras mayúsculas, números, guiones y guiones bajos."];
        }

        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 200)
        {
            errors["name"] = ["El nombre es obligatorio y debe tener entre 1 y 200 caracteres."];
        }

        if (string.IsNullOrWhiteSpace(request.Category) || !ValidCategories.Contains(request.Category.Trim().ToLowerInvariant()))
        {
            errors["category"] = [$"La categoría debe ser una de las oficiales: {string.Join(", ", ValidCategories)}."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    private static SocialServiceProgramResponse ToResponse(SocialServiceProgram p) => new(
        p.Id,
        p.Code,
        p.Name,
        p.Category,
        p.IsInternal,
        p.Description,
        p.IsActive,
        p.CreatedAt,
        p.UpdatedAt);
}
