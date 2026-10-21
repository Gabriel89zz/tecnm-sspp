namespace TecNM.Api.Core.Entities;

public sealed class Role : AuditableEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
}
