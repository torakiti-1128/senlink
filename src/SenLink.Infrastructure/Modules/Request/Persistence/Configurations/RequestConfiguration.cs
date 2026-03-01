using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Request.Entities;
using SenLink.Domain.Modules.Request.Enums;

namespace SenLink.Infrastructure.Modules.Request.Persistence.Configurations;

/// <summary>
/// 申請のテーブル構成定義
/// </summary>
public class RequestConfiguration : IEntityTypeConfiguration<Domain.Modules.Request.Entities.Request> // 名前空間と競合するため、完全修飾名で指定
{
    public void Configure(EntityTypeBuilder<Domain.Modules.Request.Entities.Request> builder)
    {
        builder.ToTable("requests");
        builder.HasKey(e => e.Id);

        // 申請者ID (NOFK, NN, accounts.id)
        builder.Property(e => e.RequesterAccountId).IsRequired();

        // 承認/差し戻し担当ID (NOFK, accounts.id)
        builder.Property(e => e.ReviewerAccountId);

        // 種別 (SMALLINT, NN)
        builder.Property(e => e.Type).IsRequired();

        // ステータス (SMALLINT, NN, Default: 0)
        builder.Property(e => e.Status).IsRequired().HasDefaultValue((RequestStatus)0);

        // 一覧表示用タイトル (VARCHAR(255), NN)
        builder.Property(e => e.Title).IsRequired().HasMaxLength(255);

        // 種別ごとの入力内容 (JSONB, NN)
        builder.Property(e => e.Payload)
            .HasColumnType("jsonb")
            .IsRequired()
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<RequestPayload>(v, (System.Text.Json.JsonSerializerOptions?)null)!
            );

        // 申請送信日時 (TIMESTAMP)
        builder.Property(e => e.SubmittedAt);

        // 承認/差し戻し確定日時 (TIMESTAMP)
        builder.Property(e => e.ResolvedAt);

        // 作成日時 (TIMESTAMP)
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
        
        // 更新日時 (TIMESTAMP)
        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
    }
}