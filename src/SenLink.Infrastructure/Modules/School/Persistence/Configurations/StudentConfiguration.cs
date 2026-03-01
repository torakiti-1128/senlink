using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.School.Entities;

namespace SenLink.Infrastructure.Modules.School.Persistence.Configurations;

/// <summary>
/// 学生のテーブル構成定義
/// </summary>
public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        // テーブル名
        builder.ToTable("students");

        // プライマリキー (PK)
        builder.HasKey(e => e.Id);

        // アカウントID (NOFK, UQ, NN)
        builder.Property(e => e.AccountId).IsRequired();
        builder.HasIndex(e => e.AccountId).IsUnique();

        // クラスID (FK, NN)
        builder.Property(e => e.ClassId).IsRequired();

        // 学籍番号 (VARCHAR(20), UQ, NN)
        builder.Property(e => e.StudentNumber).IsRequired().HasMaxLength(20);
        builder.HasIndex(e => e.StudentNumber).IsUnique();

        // 氏名 (VARCHAR(100), NN)
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);

        // 氏名カナ (VARCHAR(100), NN)
        builder.Property(e => e.NameKana).IsRequired().HasMaxLength(100);

        // 生年月日 (DATE, NN) DateOnly型は自動でDATEにマッピングされます
        builder.Property(e => e.DateOfBirth).IsRequired();

        // 性別 (SMALLINT, NN) Enumは自動で数値にマッピングされます
        builder.Property(e => e.Gender).IsRequired();

        // 入学年度 (INT, NN)
        builder.Property(e => e.AdmissionYear).IsRequired();

        // 就活中フラグ (BOOLEAN, NN, Default: TRUE)
        builder.Property(e => e.IsJobHunting).IsRequired().HasDefaultValue(true);

        // プロフィールデータ (JSONB)
        builder.Property(e => e.ProfileData)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<StudentProfile>(v, (System.Text.Json.JsonSerializerOptions?)null)
            );

        // クラスとのリレーション (1対多)
        builder.HasOne(e => e.Class)
            .WithMany(c => c.Students)
            .HasForeignKey(e => e.ClassId)
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