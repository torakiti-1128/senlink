using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Auth.Entities;

namespace SenLink.Infrastructure.Modules.Auth.Persistence.Configurations;

/// <summary>
/// Account エンティティの EF Core 設定
/// </summary>
public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        // テーブル名
        builder.ToTable("accounts");

        // 主キー
        builder.HasKey(x => x.Id);

        // 制約
        builder.Property(x => x.Email).IsRequired().HasMaxLength(255);
        builder.HasIndex(x => x.Email).IsUnique(); // ユニーク制約

        builder.Property(x => x.Password).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Role).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();

        // 共通カラム
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}