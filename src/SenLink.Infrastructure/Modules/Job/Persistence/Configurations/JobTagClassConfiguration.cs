using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Job.Entities;

namespace SenLink.Infrastructure.Modules.Job.Persistence.Configurations;

/// <summary>
/// 求人対象クラスのテーブル構成定義
/// </summary>
public class JobTargetClassConfiguration : IEntityTypeConfiguration<JobTargetClass>
{
    public void Configure(EntityTypeBuilder<JobTargetClass> builder)
    {
        // テーブル名
        builder.ToTable("job_target_classes");

        // プライマリキー (PK)
        builder.HasKey(e => e.Id);

        // 求人ID (FK, NN)
        builder.Property(e => e.JobId).IsRequired();

        // クラスID (NOFK, NN, classes.id)
        builder.Property(e => e.ClassId).IsRequired();

        // 複合ユニーク制約: UQ(job_id, class_id)
        builder.HasIndex(e => new { e.JobId, e.ClassId }).IsUnique();

        // 求人とのリレーション (多対1)
        builder.HasOne(e => e.Job)
            .WithMany(j => j.TargetClasses)
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