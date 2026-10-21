using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TecNM.Api.Core.Entities;

namespace TecNM.Api.Data.Configurations;

public sealed class CareerConfiguration : AuditableEntityConfiguration<Career>
{
    public override void Configure(EntityTypeBuilder<Career> builder)
    {
        base.Configure(builder);

        builder.ToTable("careers");
        builder.HasKey(c => c.Id).HasName("pk_careers");
        builder.Property(c => c.Code).HasColumnName("code").HasMaxLength(16).IsRequired();
        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(c => c.TotalCredits).HasColumnName("total_credits").HasDefaultValue(260).IsRequired();
        builder.Property(c => c.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();

        builder.HasIndex(c => c.Code)
            .IsUnique()
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("uq_careers__code");
    }

    protected override string GetTableName() => "careers";
}
