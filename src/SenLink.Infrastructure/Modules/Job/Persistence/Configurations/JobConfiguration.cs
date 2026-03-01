using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SenLink.Infrastructure.Modules.Job.Persistence.Configurations;

/// <summary>
/// 求人のテーブル構成定義
/// </summary>
public class JobConfiguration : IEntityTypeConfiguration<Domain.Modules.Job.Entities.Job> // 名前空間と競合するため、完全修飾名で指定
{
    public void Configure(EntityTypeBuilder<Domain.Modules.Job.Entities.Job> builder)
    {
        builder.ToTable("jobs");
        builder.HasKey(e => e.Id);

        // 企業ID (FK, NN)
        builder.Property(e => e.CompanyId).IsRequired();

        // ToDoテンプレートID (FK, NN)
        builder.Property(e => e.TodoTemplateId).IsRequired();

        // 教員アカウントID (NOFK, NN) 外部キーは張らない
        builder.Property(e => e.TeacherAccountId).IsRequired();

        // 管理用案件名 (VARCHAR(255), NN)
        builder.Property(e => e.Title).IsRequired().HasMaxLength(255);

        // 種類 (SMALLINT, NN)
        builder.Property(e => e.Type).IsRequired();

        // 開催形式 (SMALLINT, NN)
        builder.Property(e => e.Format).IsRequired();

        // 開催場所・URL (VARCHAR(255))
        builder.Property(e => e.Place).HasMaxLength(255);

        // 緊急連絡先 (VARCHAR(255))
        builder.Property(e => e.ContactInfo).HasMaxLength(255);

        // 公開範囲 (SMALLINT, NN, Default: 0)
        builder.Property(e => e.PublishScope).IsRequired().HasDefaultValue(0);

        // 企業紹介／募集要項 (TEXT, NN)
        builder.Property(e => e.Content).IsRequired().HasColumnType("text");

        // 企業とのリレーション (多対1)
        builder.HasOne(e => e.Company)
            .WithMany(c => c.Jobs)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // ToDoテンプレートとのリレーション (多対1)
        builder.HasOne(e => e.TodoTemplate)
            .WithMany(t => t.Jobs)
            .HasForeignKey(e => e.TodoTemplateId)
            .OnDelete(DeleteBehavior.Restrict);
        
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