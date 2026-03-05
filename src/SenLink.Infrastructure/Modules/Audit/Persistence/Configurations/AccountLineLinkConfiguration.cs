// AccountLineLinkConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Notification.Entities;
using SenLink.Domain.Modules.Notification.Enums;

namespace SenLink.Infrastructure.Persistence.Configurations.Notification;

/// <summary>
/// アカウントとLINEの連携情報のテーブル構成定義
/// </summary>
public class AccountLineLinkConfiguration : IEntityTypeConfiguration<AccountLineLink>
{
    public void Configure(EntityTypeBuilder<AccountLineLink> builder)
    {
        // テーブル名
        builder.ToTable("account_line_links");

        // 主キー
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        // アカウントID (NOFK, NN, UQ, accounts.id)
        builder.Property(e => e.AccountId).IsRequired();
        builder.HasIndex(e => e.AccountId).IsUnique();

        // LINEのユーザーId (VARCHAR(64), NN)
        builder.Property(e => e.LineUserId).IsRequired().HasMaxLength(64);

        // 複合ユニーク制約: UQ(account_id, line_user_id)
        builder.HasIndex(e => new { e.AccountId, e.LineUserId }).IsUnique();

        // 連携ステータス (SMALLINT, NN, Default: 0)
        builder.Property(e => e.Status).IsRequired().HasDefaultValue((LineLinkStatus)0);

        // 連携日時 (TIMESTAMP)
        builder.Property(e => e.LinkedAt);

        // 解除日時 (TIMESTAMP)
        builder.Property(e => e.UnlinkedAt);

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