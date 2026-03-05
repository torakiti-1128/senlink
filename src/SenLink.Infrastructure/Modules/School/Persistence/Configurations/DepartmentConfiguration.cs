using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.School.Entities;

namespace SenLink.Infrastructure.Modules.School.Persistence.Configurations;

/// <summary>
/// 学科のテーブル構成定義
/// </summary>
public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        // テーブル名
        builder.ToTable("departments");

        // プライマリキー (PK)
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        // 学科名 (VARCHAR(100), NN)
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        // 学科コード (VARCHAR(20), NN)
        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(20);
        
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