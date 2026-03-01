using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Activity.Entities;

namespace SenLink.Infrastructure.Modules.Activity.Persistence.Configurations;

/// <summary>
/// アクティビティのToDoリストのテーブル構成定義
/// </summary>
public class ActivityTodoConfiguration : IEntityTypeConfiguration<ActivityTodo>
{
    public void Configure(EntityTypeBuilder<ActivityTodo> builder)
    {
        // テーブル名
        builder.ToTable("activity_todos");

        // 主キー
        builder.HasKey(e => e.Id);

        // 就活ID (FK, NN)
        builder.Property(e => e.ActivityId).IsRequired();

        // タスク名 (VARCHAR(100), NN)
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);

        // 指示内容 (TEXT)
        builder.Property(e => e.Description).HasColumnType("text");

        // 順序 (INT, NN)
        builder.Property(e => e.StepOrder).IsRequired();

        // ステータス (SMALLINT, NN, Default: 0)
        builder.Property(e => e.Status).IsRequired().HasDefaultValue(0);

        // 期限日 (DATE, NN)
        builder.Property(e => e.Deadline).IsRequired();

        // 完了日時 (TIMESTAMP)
        builder.Property(e => e.CompletedAt);

        // Activity とのリレーション (多対1)
        builder.HasOne(e => e.Activity)
            .WithMany(a => a.Todos)
            .HasForeignKey(e => e.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);
        
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