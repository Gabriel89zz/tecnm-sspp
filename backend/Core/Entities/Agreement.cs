namespace TecNM.Api.Core.Entities;

public sealed class Agreement : AuditableEntity
{
    public long CompanyId { get; set; }
    public required string AgreementNumber { get; set; }
    public string AgreementType { get; set; } = "general_framework";
    public string? Description { get; set; }
    public DateOnly SignedDate { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? DocumentUrl { get; set; }

    public Company? Company { get; set; }
}
