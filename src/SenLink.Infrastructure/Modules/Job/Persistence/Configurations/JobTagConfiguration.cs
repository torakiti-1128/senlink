using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Job.Entities;

namespace SenLink.Infrastructure.Modules.Job.Persistence.Configurations;

/// <summary>
/// 求人タグ中間テーブルの構成定義
/// </summary>
public class JobTagConfiguration : IEntityTypeConfiguration<JobTag>
{
    public void Configure(EntityTypeBuilder<JobTag> builder)
    {
        // テーブル名
        builder.ToTable("job_tags");

        // プライマリキー (PK)
        builder.HasKey(e => e.Id);

        // 求人ID (FK, NN)
        builder.Property(e => e.JobId).IsRequired();

        // タグID (FK, NN)
        builder.Property(e => e.TagId).IsRequired();

        // 求人とのリレーション (多対1)
        builder.HasOne(e => e.Job)
            .WithMany(j => j.JobTags)
            .HasForeignKey(e => e.JobId)
            .OnDelete(DeleteBehavior.Cascade); // 求人が消えたらタグ紐付けも消す

        // タグとのリレーション (多対1)
        builder.HasOne(e => e.Tag)
            .WithMany(t => t.JobTags)
            .HasForeignKey(e => e.TagId)
            .OnDelete(DeleteBehavior.Cascade); // タグが消えたら紐付けも消す
        
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