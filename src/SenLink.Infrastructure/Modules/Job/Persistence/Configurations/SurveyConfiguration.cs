using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Job.Entities;

namespace SenLink.Infrastructure.Modules.Job.Persistence.Configurations;

/// <summary>
/// アンケート定義のテーブル構成定義
/// </summary>
public class SurveyConfiguration : IEntityTypeConfiguration<Survey>
{
    public void Configure(EntityTypeBuilder<Survey> builder)
    {
        // テーブル名
        builder.ToTable("surveys");

        // プライマリキー (PK)
        builder.HasKey(e => e.Id);

        // 求人ID (FK, NN)
        builder.Property(e => e.JobId).IsRequired();

        // アンケート名 (VARCHAR(255), NN)
        builder.Property(e => e.Title).IsRequired().HasMaxLength(255);

        // 質問項目 (JSONB, NN)
        builder.Property(e => e.Questions)
            .HasColumnType("jsonb")
            .IsRequired()
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<SurveyQuestions>(v, (System.Text.Json.JsonSerializerOptions?)null)!
            );

        // 求人とのリレーション (多対1)
        builder.HasOne(e => e.Job)
            .WithMany(j => j.Surveys)
            .HasForeignKey(e => e.JobId)
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