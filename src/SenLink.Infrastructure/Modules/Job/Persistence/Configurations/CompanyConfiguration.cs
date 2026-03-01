using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Job.Entities;

namespace SenLink.Infrastructure.Modules.Job.Persistence.Configurations;

/// <summary>
/// 企業のテーブル構成定義
/// </summary>
public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        // テーブル名
        builder.ToTable("companies");

        // プライマリキー (PK)
        builder.HasKey(e => e.Id);

        // 企業名 (VARCHAR(255), NN)
        builder.Property(e => e.Name).IsRequired().HasMaxLength(255);

        // 所在地 (VARCHAR(255))
        builder.Property(e => e.Address).HasMaxLength(255);

        // URL (VARCHAR(255))
        builder.Property(e => e.Url).HasMaxLength(255);

        // 作成日時 (TIMESTAMP)
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // 更新日時 (TIMESTAMP)
        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
    }
}