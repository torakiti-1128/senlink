using SenLink.Domain.Common;
using SenLink.Domain.Modules.Activity.Enums;

namespace SenLink.Domain.Modules.Activity.Entities;

/// <summary>
/// 就職活動ToDo：個別の就活に対するタスク管理
/// </summary>
public class ActivityTodo : BaseEntity
{
    // 就活ID (FK, NN)
    public long ActivityId { get; set; }

    // タスク名 (VARCHAR(100), NN)
    public string Name { get; set; } = null!;

    // 指示内容 (TEXT)
    public string? Description { get; set; }

    // 順序 (INT, NN)
    public int StepOrder { get; set; }

    // ステータス (SMALLINT, NN, Default: 0)
    public ActivityTodoStatus Status { get; set; } = ActivityTodoStatus.Incomplete;

    // 期限日 (DATE, NN)
    public DateOnly Deadline { get; set; }

    // 完了日時 (TIMESTAMP)
    public DateTime? CompletedAt { get; set; }

    // 就職活動とのリレーション
    public Activity Activity { get; set; } = null!;
}