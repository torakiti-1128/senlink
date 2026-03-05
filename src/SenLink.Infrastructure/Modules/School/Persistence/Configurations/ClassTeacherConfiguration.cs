using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.School.Entities;

namespace SenLink.Infrastructure.Modules.School.Persistence.Configurations;

/// <summary>
/// クラス担任のテーブル構成定義
/// </summary>
public class ClassTeacherConfiguration : IEntityTypeConfiguration<ClassTeacher>
{
    public void Configure(EntityTypeBuilder<ClassTeacher> builder)
    {
        // テーブル名
        builder.ToTable("class_teachers");

        // プライマリキー (PK)
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        // クラスID (FK, NN)
        builder.Property(e => e.ClassId).IsRequired();

        // 教師ID (FK, NN)
        builder.Property(e => e.TeacherId).IsRequired();

        // 役割 (SMALLINT, NN)
        builder.Property(e => e.Role).IsRequired();

        // 複合ユニーク制約: UQ(class_id, teacher_id)
        builder.HasIndex(e => new { e.ClassId, e.TeacherId }).IsUnique();

        // クラスとのリレーション (多対1)
        builder.HasOne(e => e.Class)
            .WithMany(c => c.ClassTeachers)
            .HasForeignKey(e => e.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        // 教師とのリレーション (多対1)
        builder.HasOne(e => e.Teacher)
            .WithMany(t => t.ClassTeachers)
            .HasForeignKey(e => e.TeacherId)
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