namespace TecNM.Api.Core.Entities;

public sealed class Role
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
    public long? DeletedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
}
