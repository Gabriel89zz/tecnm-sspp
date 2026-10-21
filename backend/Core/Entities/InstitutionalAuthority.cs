namespace TecNM.Api.Core.Entities;

public sealed class InstitutionalAuthority : AuditableEntity
{
    public required string RoleKey { get; set; }
    public string? Title { get; set; }
    public required string FullName { get; set; }
    public required string Position { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? SignatureInitials { get; set; }
    public bool IsActive { get; set; } = true;
}
