using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Activity.Enums;

namespace SenLink.Infrastructure.Modules.Activity.Persistence.Configurations;

/// <summary>
/// アクティビティのテーブル構成定義
/// </summary>
public class ActivityConfiguration : IEntityTypeConfiguration<Domain.Modules.Activity.Entities.Activity> // 名前空間と競合するため、完全修飾名で指定
{
    public void Configure(EntityTypeBuilder<Domain.Modules.Activity.Entities.Activity> builder)
    {
        // テーブル名
        builder.ToTable("activities");

        // 主キー
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        // 求人ID (NOFK, NN, jobs.id)
        builder.Property(e => e.JobId).IsRequired();

        // 学生ID (NOFK, NN, accounts.id)
        builder.Property(e => e.StudentAccountId).IsRequired();

        // 複合ユニーク制約: UQ(job_id, student_account_id)
        builder.HasIndex(e => new { e.JobId, e.StudentAccountId }).IsUnique();

        // ステータス (SMALLINT, NN, Default: 0)
        builder.Property(e => e.Status).IsRequired().HasDefaultValue((ActivityStatus)0);

        // 教員ID (NOFK, accounts.id)
        builder.Property(e => e.ReviewedByAccountId);

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