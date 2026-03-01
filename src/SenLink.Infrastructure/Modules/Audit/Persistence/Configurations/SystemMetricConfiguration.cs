// SystemMetricConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Audit.Entities;

namespace SenLink.Infrastructure.Persistence.Configurations.Audit;

/// <summary>
/// システムメトリクスのテーブル構成定義
/// </summary>
public class SystemMetricConfiguration : IEntityTypeConfiguration<SystemMetric>
{
    public void Configure(EntityTypeBuilder<SystemMetric> builder)
    {
        // テーブル名
        builder.ToTable("system_metrics");

        // 主キー
        builder.HasKey(e => e.Id);

        // コンポーネント (VARCHAR(50), NN)
        builder.Property(e => e.Component).IsRequired().HasMaxLength(50);

        // ステータス (SMALLINT, NN)
        builder.Property(e => e.Status).IsRequired();

        // 平均レスポンス速度(ms) (INT)
        builder.Property(e => e.ResponseTime);

        // CPU使用率(%) (NUMERIC(5,2))
        builder.Property(e => e.CpuUsage).HasColumnType("numeric(5,2)");

        // メモリ使用率(%) (NUMERIC(5,2))
        builder.Property(e => e.MemUsage).HasColumnType("numeric(5,2)");

        // ディスク使用率(%) (NUMERIC(5,2))
        builder.Property(e => e.DiskUsage).HasColumnType("numeric(5,2)");

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