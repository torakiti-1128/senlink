using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Job.Entities;

namespace SenLink.Infrastructure.Modules.Job.Persistence.Configurations;

/// <summary>
/// アンケート回答のテーブル構成定義
/// </summary>
public class SurveyResponseConfiguration : IEntityTypeConfiguration<SurveyResponse>
{
    public void Configure(EntityTypeBuilder<SurveyResponse> builder)
    {
        // テーブル名
        builder.ToTable("survey_responses");

        // プライマリキー (PK)
        builder.HasKey(e => e.Id);

        // アンケート定義ID (FK, NN)
        builder.Property(e => e.SurveyId).IsRequired();

        // 学生ID (NOFK, NN, accounts.id)
        builder.Property(e => e.StudentAccountId).IsRequired();

        // 回答内容 (JSONB, NN)
        builder.OwnsOne(e => e.Answers, a =>
        {
            a.ToJson(); // PostgreSQLのJSONBカラムとしてマッピング
        });

        // アンケート定義とのリレーション (多対1)
        builder.HasOne(e => e.Survey)
            .WithMany(s => s.Responses)
            .HasForeignKey(e => e.SurveyId)
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