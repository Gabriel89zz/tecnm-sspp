using Microsoft.EntityFrameworkCore;
using TecNM.Api.Core.Entities;
using TecNM.Api.Core.Exceptions;
using TecNM.Api.Core.Http;
using TecNM.Api.Data;

namespace TecNM.Api.Modules.Companies;

public class SupervisorsService(TecNMDbContext dbContext)
{
    public async Task<IReadOnlyList<CompanySupervisorResponse>> GetByCompanyAsync(
        long companyId,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        await EnsureCompanyExistsAsync(companyId, cancellationToken);

        var query = dbContext.CompanySupervisors
            .AsNoTracking()
            .Where(s => s.CompanyId == companyId);

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(s => s.FullName)
            .Select(s => ToResponse(s))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OptionItem>> GetOptionsByCompanyAsync(
        long companyId,
        CancellationToken cancellationToken = default)
    {
        await EnsureCompanyExistsAsync(companyId, cancellationToken);

        var items = await dbContext.CompanySupervisors
            .AsNoTracking()
            .Where(s => s.CompanyId == companyId && s.IsActive)
            .OrderBy(s => s.FullName)
            .Select(s => new { s.Id, s.FullName, s.Position, s.IsSignatory })
            .ToListAsync(cancellationToken);

        return items
            .Select(s => new OptionItem(
                s.Id,
                $"{s.FullName} · {s.Position}",
                new Dictionary<string, object?> { ["isSignatory"] = s.IsSignatory }))
            .ToList();
    }

    public async Task<CompanySupervisorResponse> GetByIdAsync(
        long companyId,
        long id,
        CancellationToken cancellationToken = default)
    {
        await EnsureCompanyExistsAsync(companyId, cancellationToken);

        var supervisor = await dbContext.CompanySupervisors
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.CompanyId == companyId && s.Id == id, cancellationToken);

        if (supervisor is null)
        {
            throw new NotFoundException("company_supervisors.not_found", $"Supervisor con id {id} no encontrado en la empresa especificada.");
        }

        return ToResponse(supervisor);
    }

    public async Task<CompanySupervisorResponse> CreateAsync(
        long companyId,
        CompanySupervisorRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureCompanyExistsAsync(companyId, cancellationToken);
        ValidateRequest(request);

        var supervisor = new CompanySupervisor
        {
            CompanyId = companyId,
            FullName = request.FullName.Trim(),
            Position = request.Position.Trim(),
            Department = string.IsNullOrWhiteSpace(request.Department) ? null : request.Department.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant(),
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            IsSignatory = request.IsSignatory,
            IsActive = true
        };

        dbContext.CompanySupervisors.Add(supervisor);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(supervisor);
    }

    public async Task<CompanySupervisorResponse> UpdateAsync(
        long companyId,
        long id,
        CompanySupervisorRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureCompanyExistsAsync(companyId, cancellationToken);
        ValidateRequest(request);

        var supervisor = await dbContext.CompanySupervisors
            .SingleOrDefaultAsync(s => s.CompanyId == companyId && s.Id == id, cancellationToken);

        if (supervisor is null)
        {
            throw new NotFoundException("company_supervisors.not_found", $"Supervisor con id {id} no encontrado en la empresa especificada.");
        }

        supervisor.FullName = request.FullName.Trim();
        supervisor.Position = request.Position.Trim();
        supervisor.Department = string.IsNullOrWhiteSpace(request.Department) ? null : request.Department.Trim();
        supervisor.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant();
        supervisor.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        supervisor.IsSignatory = request.IsSignatory;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(supervisor);
    }

    public async Task<CompanySupervisorResponse> UpdateStatusAsync(
        long companyId,
        long id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await EnsureCompanyExistsAsync(companyId, cancellationToken);

        var supervisor = await dbContext.CompanySupervisors
            .SingleOrDefaultAsync(s => s.CompanyId == companyId && s.Id == id, cancellationToken);

        if (supervisor is null)
        {
            throw new NotFoundException("company_supervisors.not_found", $"Supervisor con id {id} no encontrado en la empresa especificada.");
        }

        supervisor.IsActive = isActive;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(supervisor);
    }

    public async Task DeleteAsync(long companyId, long id, CancellationToken cancellationToken = default)
    {
        await EnsureCompanyExistsAsync(companyId, cancellationToken);

        var supervisor = await dbContext.CompanySupervisors
            .SingleOrDefaultAsync(s => s.CompanyId == companyId && s.Id == id, cancellationToken);

        if (supervisor is null)
        {
            throw new NotFoundException("company_supervisors.not_found", $"Supervisor con id {id} no encontrado en la empresa especificada.");
        }

        dbContext.CompanySupervisors.Remove(supervisor);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureCompanyExistsAsync(long companyId, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Companies.AnyAsync(c => c.Id == companyId, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException("companies.not_found", $"Empresa con id {companyId} no encontrada.");
        }
    }

    private static void ValidateRequest(CompanySupervisorRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Trim().Length > 180)
        {
            errors["fullName"] = ["El nombre completo es obligatorio y debe tener entre 1 y 180 caracteres."];
        }

        if (string.IsNullOrWhiteSpace(request.Position) || request.Position.Trim().Length > 150)
        {
            errors["position"] = ["El puesto o cargo es obligatorio y debe tener entre 1 y 150 caracteres."];
        }

        if (request.Department?.Trim().Length > 150)
        {
            errors["department"] = ["El departamento no puede exceder los 150 caracteres."];
        }

        if (request.PhoneNumber?.Trim().Length > 30)
        {
            errors["phoneNumber"] = ["El número de teléfono no puede exceder los 30 caracteres."];
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = request.Email.Trim();
            if (email.Length > 255 || !email.Contains('@') || !email.Contains('.'))
            {
                errors["email"] = ["El correo electrónico no es válido o supera los 255 caracteres."];
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    private static CompanySupervisorResponse ToResponse(CompanySupervisor s) =>
        new(
            s.Id,
            s.CompanyId,
            s.FullName,
            s.Position,
            s.Department,
            s.Email,
            s.PhoneNumber,
            s.IsSignatory,
            s.IsActive,
            s.CreatedAt,
            s.UpdatedAt);
}
