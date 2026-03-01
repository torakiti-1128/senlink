// ErrorLogConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Audit.Entities;

namespace SenLink.Infrastructure.Persistence.Configurations.Audit;

/// <summary>
/// エラーログのテーブル構成定義
/// </summary>
public class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
{
    public void Configure(EntityTypeBuilder<ErrorLog> builder)
    {
        // テーブル名
        builder.ToTable("error_logs");

        // 主キー
        builder.HasKey(e => e.Id);

        // 発生元サービス (VARCHAR(50), NN)
        builder.Property(e => e.ServiceName).IsRequired().HasMaxLength(50);

        // 深刻度 (SMALLINT, NN)
        builder.Property(e => e.Severity).IsRequired();

        // エラーメッセージの要約 (TEXT, NN)
        builder.Property(e => e.Message).IsRequired().HasColumnType("text");

        // 詳細なスタックトレース (TEXT)
        builder.Property(e => e.StackTrace).HasColumnType("text");

        // 発生時のAPIエンドポイントURL (TEXT)
        builder.Property(e => e.RequestUrl).HasColumnType("text");

        // 発生時のリクエストボディ、クエリ等 (JSONB)
        builder.Property(e => e.RequestParams)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<RequestParams>(v, (System.Text.Json.JsonSerializerOptions?)null)
            );

        // 発生時にログインしていたアカウントID (NOFK, accounts.id)
        builder.Property(e => e.AccountId);

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