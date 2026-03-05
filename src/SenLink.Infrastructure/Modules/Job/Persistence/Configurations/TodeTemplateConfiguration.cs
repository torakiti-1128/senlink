using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Job.Entities;

namespace SenLink.Infrastructure.Modules.Job.Persistence.Configurations;

/// <summary>
/// ToDoテンプレートのテーブル構成定義
/// </summary>
public class TodoTemplateConfiguration : IEntityTypeConfiguration<TodoTemplate>
{
    public void Configure(EntityTypeBuilder<TodoTemplate> builder)
    {
        // テーブル名
        builder.ToTable("todo_templates");

        // プライマリキー (PK)
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        // テンプレート名 (VARCHAR(100), NN)
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);

        // 説明 (TEXT)
        builder.Property(e => e.Description).HasColumnType("text");

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