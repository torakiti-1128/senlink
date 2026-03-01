using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Job.Entities;

namespace SenLink.Infrastructure.Modules.Job.Persistence.Configurations;

/// <summary>
/// ブックマークのテーブル構成定義
/// </summary>
public class BookmarkConfiguration : IEntityTypeConfiguration<Bookmark>
{
    public void Configure(EntityTypeBuilder<Bookmark> builder)
    {
        // テーブル名
        builder.ToTable("bookmarks");

        // プライマリキー (PK)
        builder.HasKey(e => e.Id);

        // 求人ID (FK, NN)
        builder.Property(e => e.JobId).IsRequired();

        // 学生ID (NOFK, NN, accounts.id)
        builder.Property(e => e.StudentAccountId).IsRequired();

        // 複合ユニーク制約: UQ(job_id, student_account_id)
        builder.HasIndex(e => new { e.JobId, e.StudentAccountId }).IsUnique();

        // 求人とのリレーション (多対1)
        builder.HasOne(e => e.Job)
            .WithMany(j => j.Bookmarks)
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