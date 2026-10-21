using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TecNM.Api.Core.Entities;

namespace TecNM.Api.Data.Configurations;

public sealed class RoleConfiguration : AuditableEntityConfiguration<Role>
{
    public override void Configure(EntityTypeBuilder<Role> builder)
    {
        base.Configure(builder);

        builder.ToTable("roles");
        builder.HasKey(r => r.Id).HasName("pk_roles");
        builder.Property(r => r.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
        builder.Property(r => r.Description).HasColumnName("description");

        // Partial unique index en PostgreSQL: uq_roles__name WHERE deleted_at IS NULL
        builder.HasIndex(r => r.Name)
            .IsUnique()
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("uq_roles__name");
    }

    protected override string GetTableName() => "roles";
}
