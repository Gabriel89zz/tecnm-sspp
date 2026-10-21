namespace TecNM.Api.Core.Entities;

public sealed class SocialServiceProgram : AuditableEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public bool IsInternal { get; set; } = true;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
