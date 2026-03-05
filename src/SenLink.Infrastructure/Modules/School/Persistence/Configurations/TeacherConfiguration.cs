using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.School.Entities;

namespace SenLink.Infrastructure.Modules.School.Persistence.Configurations;

/// <summary>
/// 教員のテーブル構成定義
/// </summary>
public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        // テーブル名
        builder.ToTable("teachers");

        // プライマリキー (PK)
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        // アカウントID (NOFK, UQ, NN) 外部キーは張らずにユニーク制約のみ付与
        builder.Property(e => e.AccountId).IsRequired();
        builder.HasIndex(e => e.AccountId).IsUnique();

        // 氏名 (VARCHAR(100), NN)
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);

        // 氏名カナ (VARCHAR(100), NN)
        builder.Property(e => e.NameKana).IsRequired().HasMaxLength(100);

        // 役職 (VARCHAR(50))
        builder.Property(e => e.Title).HasMaxLength(50);

        // オフィス場所 (VARCHAR(100))
        builder.Property(e => e.OfficeLocation).HasMaxLength(100);

        // プロフィールデータ (JSONB) .NET 8のJSONマッピング機能を使用
        builder.Property(e => e.ProfileData)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<TeacherProfile>(v, (System.Text.Json.JsonSerializerOptions?)null)
            );

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