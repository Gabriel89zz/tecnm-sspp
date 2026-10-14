using Microsoft.EntityFrameworkCore;
using TecNM.Api.Core.Entities;

namespace TecNM.Api.Data;

public sealed class TecNMDbContext(DbContextOptions<TecNMDbContext> options) : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var role = modelBuilder.Entity<Role>();
        role.ToTable("roles");
        role.HasKey(item => item.Id).HasName("pk_roles");
        role.Property(item => item.Id).HasColumnName("id").UseIdentityByDefaultColumn();
        role.Property(item => item.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
        role.Property(item => item.Description).HasColumnName("description");
        role.Property(item => item.CreatedBy).HasColumnName("created_by");
        role.Property(item => item.UpdatedBy).HasColumnName("updated_by");
        role.Property(item => item.DeletedBy).HasColumnName("deleted_by");
        role.Property(item => item.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        role.Property(item => item.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        role.Property(item => item.DeletedAt).HasColumnName("deleted_at");
        role.HasIndex(item => item.Name).IsUnique().HasDatabaseName("uq_roles__name");
        role.HasIndex(item => item.DeletedAt).HasDatabaseName("idx_roles__deleted_at");

        var user = modelBuilder.Entity<User>();
        user.ToTable("users");
        user.HasKey(item => item.Id).HasName("pk_users");
        user.Property(item => item.Id).HasColumnName("id").UseIdentityByDefaultColumn();
        user.Property(item => item.Username).HasColumnName("username").HasMaxLength(64).IsRequired();
        user.Property(item => item.Email).HasColumnName("email").HasMaxLength(254).IsRequired();
        user.Property(item => item.PasswordHash).HasColumnName("password_hash").HasMaxLength(512).IsRequired();
        user.Property(item => item.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
        user.Property(item => item.FirstSurname).HasColumnName("first_surname").HasMaxLength(70).IsRequired();
        user.Property(item => item.SecondSurname).HasColumnName("second_surname").HasMaxLength(70);
        user.Property(item => item.IsActive).HasColumnName("is_active").IsRequired();
        user.Property(item => item.CreatedBy).HasColumnName("created_by");
        user.Property(item => item.UpdatedBy).HasColumnName("updated_by");
        user.Property(item => item.DeletedBy).HasColumnName("deleted_by");
        user.Property(item => item.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        user.Property(item => item.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        user.Property(item => item.DeletedAt).HasColumnName("deleted_at");
        user.HasIndex(item => item.Username).IsUnique().HasDatabaseName("uq_users__username");
        user.HasIndex(item => item.Email).IsUnique().HasDatabaseName("uq_users__email");
        user.HasIndex(item => item.DeletedAt).HasDatabaseName("idx_users__deleted_at");

        var userRole = modelBuilder.Entity<UserRole>();
        userRole.ToTable("user_roles");
        userRole.HasKey(item => item.Id).HasName("pk_user_roles");
        userRole.Property(item => item.Id).HasColumnName("id").UseIdentityByDefaultColumn();
        userRole.Property(item => item.UserId).HasColumnName("user_id").IsRequired();
        userRole.Property(item => item.RoleId).HasColumnName("role_id").IsRequired();
        userRole.Property(item => item.CreatedBy).HasColumnName("created_by");
        userRole.Property(item => item.UpdatedBy).HasColumnName("updated_by");
        userRole.Property(item => item.DeletedBy).HasColumnName("deleted_by");
        userRole.Property(item => item.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        userRole.Property(item => item.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        userRole.Property(item => item.DeletedAt).HasColumnName("deleted_at");
        userRole.HasIndex(item => new { item.UserId, item.RoleId }).IsUnique().HasDatabaseName("uq_user_roles__user_role");
        userRole.HasIndex(item => item.UserId).HasDatabaseName("idx_user_roles__user_id");
        userRole.HasIndex(item => item.RoleId).HasDatabaseName("idx_user_roles__role_id");
        userRole.HasIndex(item => item.DeletedAt).HasDatabaseName("idx_user_roles__deleted_at");

        userRole.HasOne(item => item.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(item => item.UserId)
            .HasConstraintName("fk_user_roles__users")
            .OnDelete(DeleteBehavior.Cascade);

        userRole.HasOne(item => item.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(item => item.RoleId)
            .HasConstraintName("fk_user_roles__roles")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
