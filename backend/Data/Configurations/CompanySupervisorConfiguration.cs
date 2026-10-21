using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TecNM.Api.Core.Entities;

namespace TecNM.Api.Data.Configurations;

public sealed class CompanySupervisorConfiguration : AuditableEntityConfiguration<CompanySupervisor>
{
    public override void Configure(EntityTypeBuilder<CompanySupervisor> builder)
    {
        base.Configure(builder);

        builder.ToTable("company_supervisors");
        builder.HasKey(s => s.Id).HasName("pk_company_supervisors");

        builder.Property(s => s.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(s => s.FullName).HasColumnName("full_name").HasMaxLength(180).IsRequired();
        builder.Property(s => s.Position).HasColumnName("position").HasMaxLength(150).IsRequired();
        builder.Property(s => s.Department).HasColumnName("department").HasMaxLength(150);
        builder.Property(s => s.Email).HasColumnName("email").HasMaxLength(255);
        builder.Property(s => s.PhoneNumber).HasColumnName("phone_number").HasMaxLength(30);
        builder.Property(s => s.IsSignatory).HasColumnName("is_signatory").HasDefaultValue(true).IsRequired();
        builder.Property(s => s.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();

        builder.HasIndex(s => s.CompanyId).HasDatabaseName("idx_company_supervisors__company_id");
    }

    protected override string GetTableName() => "company_supervisors";
}
