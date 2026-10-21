using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TecNM.Api.Core.Entities;

namespace TecNM.Api.Data.Configurations;

public sealed class AgreementConfiguration : AuditableEntityConfiguration<Agreement>
{
    public override void Configure(EntityTypeBuilder<Agreement> builder)
    {
        base.Configure(builder);

        builder.ToTable("agreements");
        builder.HasKey(a => a.Id).HasName("pk_agreements");

        builder.Property(a => a.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(a => a.AgreementNumber).HasColumnName("agreement_number").HasMaxLength(64).IsRequired();
        builder.Property(a => a.AgreementType).HasColumnName("agreement_type").HasMaxLength(32).HasDefaultValue("general_framework").IsRequired();
        builder.Property(a => a.Description).HasColumnName("description");
        builder.Property(a => a.SignedDate).HasColumnName("signed_date").HasColumnType("date").IsRequired();
        builder.Property(a => a.ExpirationDate).HasColumnName("expiration_date").HasColumnType("date").IsRequired();
        builder.Property(a => a.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();
        builder.Property(a => a.DocumentUrl).HasColumnName("document_url");

        builder.HasIndex(a => a.AgreementNumber)
            .IsUnique()
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("uq_agreements__agreement_number");

        builder.HasIndex(a => a.CompanyId).HasDatabaseName("idx_agreements__company_id");
        builder.HasIndex(a => a.IsActive).HasDatabaseName("idx_agreements__is_active");
    }

    protected override string GetTableName() => "agreements";
}
