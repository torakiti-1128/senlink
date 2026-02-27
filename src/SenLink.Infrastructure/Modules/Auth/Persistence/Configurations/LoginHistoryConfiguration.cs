using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Auth.Entities;

namespace SenLink.Infrastructure.Modules.Auth.Persistence.Configurations;

/// <summary>
/// LoginHistory エンティティの EF Core 設定
/// </summary>
public class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        // テーブル名指定
        builder.ToTable("login_histories");

        // 主キー
        builder.HasKey(x => x.Id);

        // アカウントID (FK, NN)
        builder.Property(x => x.AccountId)
            .IsRequired();

        // 同一モジュール（Auth）内なので物理的な外部キー制約を定義
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade); // アカウント削除時に履歴も削除

        // IPアドレス (VARCHAR(45))
        builder.Property(x => x.IpAddress)
            .HasMaxLength(45);

        // ブラウザ情報 (TEXT)
        builder.Property(x => x.UserAgent)
            .HasColumnType("text");

        // ステータス (SMALLINT)
        builder.Property(x => x.Status)
            .IsRequired();

        // 共通カラムの設定
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
    }
}