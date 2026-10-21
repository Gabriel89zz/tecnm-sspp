namespace TecNM.Api.Core.Entities;

public sealed class CompanySupervisor : AuditableEntity
{
    public long CompanyId { get; set; }
    public required string FullName { get; set; }
    public required string Position { get; set; }
    public string? Department { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsSignatory { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public Company? Company { get; set; }
}
