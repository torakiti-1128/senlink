using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Notification.Enums;

namespace SenLink.Infrastructure.Modules.Notification.Persistence.Configurations;

/// <summary>
/// 通知のテーブル構成定義
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Domain.Modules.Notification.Entities.Notification> // 名前空間と競合するため、完全修飾名で指定します
{
    public void Configure(EntityTypeBuilder<Domain.Modules.Notification.Entities.Notification> builder)
    {
        // テーブル名
        builder.ToTable("notifications");

        // 主キー
        builder.HasKey(e => e.Id);

        // 受信者ID (NOFK, NN)
        builder.Property(e => e.RecipientAccountId).IsRequired();

        // タイトル (VARCHAR(255), NN)
        builder.Property(e => e.Title).IsRequired().HasMaxLength(255);

        // 本文 (TEXT)
        builder.Property(e => e.Body).HasColumnType("text");

        // 遷移URL (VARCHAR(255))
        builder.Property(e => e.LinkUrl).HasMaxLength(255);

        // 種別 (SMALLINT, NN)
        builder.Property(e => e.Type).IsRequired();

        // 既読ステータス (SMALLINT, NN, Default: 0)
        builder.Property(e => e.ReadStatus).IsRequired().HasDefaultValue((ReadStatus)0);

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