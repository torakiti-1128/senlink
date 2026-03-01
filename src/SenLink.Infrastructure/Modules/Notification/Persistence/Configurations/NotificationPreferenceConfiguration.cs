using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Notification.Entities;

namespace SenLink.Infrastructure.Modules.Notification.Persistence.Configurations;

/// <summary>
/// 通知設定のテーブル構成定義
/// </summary>
public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        // テーブル名
        builder.ToTable("notification_preferences");
        
        // 主キーはアカウントID
        builder.HasKey(e => e.AccountId);
        builder.Property(e => e.AccountId).IsRequired();

        // 通知センター有効フラグ (BOOLEAN, NN, Default: true)
        builder.Property(e => e.InAppEnabled).IsRequired().HasDefaultValue(true);

        // メール有効フラグ (BOOLEAN, NN, Default: true)
        builder.Property(e => e.EmailEnabled).IsRequired().HasDefaultValue(true);

        // LINE有効フラグ (BOOLEAN, NN, Default: false)
        builder.Property(e => e.LineEnabled).IsRequired().HasDefaultValue(false);

        // 全停止フラグ (BOOLEAN, NN, Default: false)
        builder.Property(e => e.MuteAll).IsRequired().HasDefaultValue(false);

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