// SystemSettingConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Maintenance.Entities;
using SenLink.Domain.Modules.Maintenance.Enums;

namespace SenLink.Infrastructure.Persistence.Configurations.Maintenance;

/// <summary>
/// システム設定のテーブル構成定義
/// </summary>
public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        // テーブル名
        builder.ToTable("system_settings");

        // 主キー
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        // 設定キー (VARCHAR(50), UQ, NN)
        builder.Property(e => e.Key).IsRequired().HasMaxLength(50);
        builder.HasIndex(e => e.Key).IsUnique();

        // 現在の設定値 (TEXT, NN)
        builder.Property(e => e.Value).IsRequired().HasColumnType("text");

        // データ型 (SMALLINT, NN, Default: 0)
        builder.Property(e => e.ValueType).IsRequired().HasDefaultValue((SettingValueType)0);

        // 説明 (VARCHAR(255))
        builder.Property(e => e.Description).HasMaxLength(255);

        // 更新回数 (INT, NN, Default: 0)
        builder.Property(e => e.ChangeCounts).IsRequired().HasDefaultValue(0);

        // 機密設定 (BOOLEAN, NN, Default: false)
        builder.Property(e => e.IsSensitive).IsRequired().HasDefaultValue(false);

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