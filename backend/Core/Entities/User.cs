namespace TecNM.Api.Core.Entities;

public sealed class User
{
    public long Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string FirstName { get; set; }
    public required string FirstSurname { get; set; }
    public string? SecondSurname { get; set; }
    public string FullName => string.IsNullOrWhiteSpace(SecondSurname)
        ? $"{FirstName} {FirstSurname}"
        : $"{FirstName} {FirstSurname} {SecondSurname}";
    public bool IsActive { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
    public long? DeletedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
}
