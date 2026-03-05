using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Job.Entities;

namespace SenLink.Infrastructure.Modules.Job.Persistence.Configurations;

/// <summary>
/// ToDoステップのテーブル構成定義
/// </summary>
public class TodoStepConfiguration : IEntityTypeConfiguration<TodoStep>
{
    public void Configure(EntityTypeBuilder<TodoStep> builder)
    {
        // テーブル名
        builder.ToTable("todo_steps");

        // プライマリキー (PK)
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        // 親テンプレートID (FK, NN)
        builder.Property(e => e.TemplateId).IsRequired();

        // タスク名 (VARCHAR(100), NN)
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);

        // 指示内容 (TEXT)
        builder.Property(e => e.Description).HasColumnType("text");

        // 順序 (INT, NN)
        builder.Property(e => e.StepOrder).IsRequired();

        // 相対期限 (INT, Default: 0)
        builder.Property(e => e.DaysDeadline).HasDefaultValue(0);

        // 承認必須フラグ (BOOLEAN, Default: FALSE)
        builder.Property(e => e.IsVerificationRequired).HasDefaultValue(false);

        // ToDoテンプレートとのリレーション (多対1)
        builder.HasOne(e => e.Template)
            .WithMany(t => t.Steps)
            .HasForeignKey(e => e.TemplateId)
            .OnDelete(DeleteBehavior.Cascade); // テンプレートが消えたらステップも消す
        
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