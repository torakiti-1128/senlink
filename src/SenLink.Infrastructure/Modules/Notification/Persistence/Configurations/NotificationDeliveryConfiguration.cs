using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Notification.Entities;
using SenLink.Domain.Modules.Notification.Enums;

namespace SenLink.Infrastructure.Modules.Notification.Persistence.Configurations;

/// <summary>
/// 送達状態のテーブル構成定義
/// </summary>
public class NotificationDeliveryConfiguration : IEntityTypeConfiguration<NotificationDelivery>
{
    public void Configure(EntityTypeBuilder<NotificationDelivery> builder)
    {
        // テーブル名
        builder.ToTable("notification_deliveries");

        // 主キー
        builder.HasKey(e => e.Id);

        // 通知ID (FK, NN)
        builder.Property(e => e.NotificationId).IsRequired();

        // 受信者ID (NOFK, NN)
        builder.Property(e => e.RecipientAccountId).IsRequired();

        // 送信チャネル (SMALLINT, NN)
        builder.Property(e => e.Channel).IsRequired();

        // 送信ステータス (SMALLINT, NN, Default: 0)
        builder.Property(e => e.Status).IsRequired().HasDefaultValue((DeliveryStatus)0);

        // 外部事業者のメッセージID (VARCHAR(128))
        builder.Property(e => e.ProviderMessageId).HasMaxLength(128);

        // 失敗種別 (VARCHAR(64))
        builder.Property(e => e.ErrorType).HasMaxLength(64);

        // エラー詳細 (TEXT)
        builder.Property(e => e.ErrorMessage).HasColumnType("text");

        // 送信試行回数 (INT, NN, Default: 0)
        builder.Property(e => e.AttemptCount).IsRequired().HasDefaultValue(0);

        // 次回リトライ時刻 (TIMESTAMP)
        builder.Property(e => e.NextRetryAt);

        // Notification とのリレーション (多対1)
        builder.HasOne(e => e.Notification)
            .WithMany(n => n.Deliveries)
            .HasForeignKey(e => e.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);
        
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