using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TecNM.Api.Core.Entities;

namespace TecNM.Api.Data.Configurations;

public sealed class SocialServiceProgramConfiguration : AuditableEntityConfiguration<SocialServiceProgram>
{
    public override void Configure(EntityTypeBuilder<SocialServiceProgram> builder)
    {
        base.Configure(builder);

        builder.ToTable("social_service_programs");
        builder.HasKey(p => p.Id).HasName("pk_social_service_programs");
        builder.Property(p => p.Code).HasColumnName("code").HasMaxLength(32).IsRequired();
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.Category).HasColumnName("category").HasMaxLength(64).IsRequired();
        builder.Property(p => p.IsInternal).HasColumnName("is_internal").HasDefaultValue(true).IsRequired();
        builder.Property(p => p.Description).HasColumnName("description");
        builder.Property(p => p.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();

        builder.HasIndex(p => p.Code)
            .IsUnique()
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("uq_ss_programs__code");

        builder.HasIndex(p => p.Category).HasDatabaseName("idx_ss_programs__category");
    }

    protected override string GetTableName() => "social_service_programs";
}
