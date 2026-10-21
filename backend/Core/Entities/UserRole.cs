namespace TecNM.Api.Core.Entities;

public sealed class UserRole : AuditableEntity
{
    public long UserId { get; set; }
    public User? User { get; set; }
    public long RoleId { get; set; }
    public Role? Role { get; set; }
}
