using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TecNM.Api.Core.Entities;

namespace TecNM.Api.Data.Configurations;

public sealed class InstitutionalAuthorityConfiguration : AuditableEntityConfiguration<InstitutionalAuthority>
{
    public override void Configure(EntityTypeBuilder<InstitutionalAuthority> builder)
    {
        base.Configure(builder);

        builder.ToTable("institutional_authorities");
        builder.HasKey(a => a.Id).HasName("pk_institutional_authorities");
        builder.Property(a => a.RoleKey).HasColumnName("role_key").HasMaxLength(64).IsRequired();
        builder.Property(a => a.Title).HasColumnName("title").HasMaxLength(32);
        builder.Property(a => a.FullName).HasColumnName("full_name").HasMaxLength(180).IsRequired();
        builder.Property(a => a.Position).HasColumnName("position").HasMaxLength(180).IsRequired();
        builder.Property(a => a.Email).HasColumnName("email").HasMaxLength(255);
        builder.Property(a => a.PhoneNumber).HasColumnName("phone_number").HasMaxLength(30);
        builder.Property(a => a.SignatureInitials).HasColumnName("signature_initials").HasMaxLength(16);
        builder.Property(a => a.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();

        builder.HasIndex(a => a.RoleKey).HasDatabaseName("idx_institutional_authorities__role_key");
    }

    protected override string GetTableName() => "institutional_authorities";
}
