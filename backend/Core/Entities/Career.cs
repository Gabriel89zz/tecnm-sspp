namespace TecNM.Api.Core.Entities;

public sealed class Career : AuditableEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public int TotalCredits { get; set; } = 260;
    public bool IsActive { get; set; } = true;
}
