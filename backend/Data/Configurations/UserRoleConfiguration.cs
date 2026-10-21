using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TecNM.Api.Core.Entities;

namespace TecNM.Api.Data.Configurations;

public sealed class UserRoleConfiguration : AuditableEntityConfiguration<UserRole>
{
    public override void Configure(EntityTypeBuilder<UserRole> builder)
    {
        base.Configure(builder);

        builder.ToTable("user_roles");
        builder.HasKey(ur => ur.Id).HasName("pk_user_roles");
        builder.Property(ur => ur.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(ur => ur.RoleId).HasColumnName("role_id").IsRequired();

        builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique()
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("uq_user_roles__user_role");

        builder.HasIndex(ur => ur.UserId).HasDatabaseName("idx_user_roles__user_id");
        builder.HasIndex(ur => ur.RoleId).HasDatabaseName("idx_user_roles__role_id");

        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .HasConstraintName("fk_user_roles__users")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .HasConstraintName("fk_user_roles__roles")
            .OnDelete(DeleteBehavior.Restrict);
    }

    protected override string GetTableName() => "user_roles";
}
