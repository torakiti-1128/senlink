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

        // メールアドレス (VARCHAR(255), NN, UNIQUE)
        builder.Property(x => x.Email).IsRequired().HasMaxLength(255);
        builder.HasIndex(x => x.Email).IsUnique(); // ユニーク制約

        // パスワードはハッシュ化して保存する (VARCHAR(255))
        builder.Property(x => x.Password).IsRequired().HasMaxLength(255);

        // ロール (SMALLINT)
        builder.Property(x => x.Role).IsRequired();

        // アクティブフラグ (BOOLEAN)
        builder.Property(x => x.IsActive).IsRequired();

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