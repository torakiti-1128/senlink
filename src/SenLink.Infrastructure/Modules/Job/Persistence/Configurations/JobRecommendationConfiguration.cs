using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Job.Entities;

namespace SenLink.Infrastructure.Modules.Job.Persistence.Configurations;

/// <summary>
/// 求人推薦のテーブル構成定義
/// </summary>
public class JobRecommendationConfiguration : IEntityTypeConfiguration<JobRecommendation>
{
    public void Configure(EntityTypeBuilder<JobRecommendation> builder)
    {
        // テーブル名
        builder.ToTable("job_recommendations");

        // プライマリキー (PK)
        builder.HasKey(e => e.Id);

        // 求人ID (FK, NN)
        builder.Property(e => e.JobId).IsRequired();

        // 学生ID (NOFK, NN, accounts.id)
        builder.Property(e => e.StudentAccountId).IsRequired();

        // 教員ID (NOFK, NN, accounts.id)
        builder.Property(e => e.RecommenderAccountId).IsRequired();

        // 求人とのリレーション (多対1)
        builder.HasOne(e => e.Job)
            .WithMany(j => j.Recommendations)
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