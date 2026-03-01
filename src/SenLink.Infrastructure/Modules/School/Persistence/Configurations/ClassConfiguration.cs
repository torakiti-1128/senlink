using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.School.Entities;

namespace SenLink.Infrastructure.Modules.School.Persistence.Configurations;

/// <summary>
/// クラスのテーブル構成定義
/// </summary>
public class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        // テーブル名
        builder.ToTable("classes");

        // プライマリキー (PK)
        builder.HasKey(e => e.Id);

        // 学科ID (FK, NN)
        builder.Property(e => e.DepartmentId).IsRequired();

        // 年度 (SMALLINT, NN)
        builder.Property(e => e.FiscalYear).IsRequired();

        // 学年 (SMALLINT, NN)
        builder.Property(e => e.Grade).IsRequired();

        // クラス名 (VARCHAR(50), NN)
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(50);

        // 学科とのリレーション (1対多)
        builder.HasOne(e => e.Department)
            .WithMany(d => d.Classes)
            .HasForeignKey(e => e.DepartmentId)
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