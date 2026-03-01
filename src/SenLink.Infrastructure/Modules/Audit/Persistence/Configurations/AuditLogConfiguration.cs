// AuditLogConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Audit.Entities;

namespace SenLink.Infrastructure.Persistence.Configurations.Audit;

/// <summary>
/// 監査ログのテーブル構成定義
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        // テーブル名
        builder.ToTable("audit_logs");

        // 主キー
        builder.HasKey(e => e.Id);

        // 操作者ID (NOFK, NN, accounts.id)
        builder.Property(e => e.ActorId).IsRequired();

        // 対象テーブル名 (VARCHAR(50), NN)
        builder.Property(e => e.TargetTable).IsRequired().HasMaxLength(50);

        // 対象ID (NN)
        builder.Property(e => e.TargetId).IsRequired();

        // 操作種別 (VARCHAR(50), NN)
        builder.Property(e => e.Method).IsRequired().HasMaxLength(50);

        // 変更前後のデータ (JSONB)
        builder.Property(e => e.Details)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<AuditLogDetails>(v, (System.Text.Json.JsonSerializerOptions?)null)
            );

        // IPアドレス (VARCHAR(45))
        builder.Property(e => e.IpAddress).HasMaxLength(45);

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