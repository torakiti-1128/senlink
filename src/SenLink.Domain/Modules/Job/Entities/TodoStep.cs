using SenLink.Domain.Common;

namespace SenLink.Domain.Modules.Job.Entities;

/// <summary>
/// ToDoステップ：テンプレートに属する個別のタスク
/// </summary>
public class TodoStep : BaseEntity
{
    // 親テンプレートID (FK, NN)
    public long TemplateId { get; set; }

    // タスク名 (VARCHAR(100), NN)
    public string Name { get; set; } = null!;

    // 指示内容 (TEXT)
    public string? Description { get; set; }

    // 順序 (INT, NN)
    public int StepOrder { get; set; }

    // 相対期限（日） (INT, Default: 0)
    public int DaysDeadline { get; set; } = 0;

    // 承認必須フラグ (BOOLEAN, Default: FALSE)
    public bool IsVerificationRequired { get; set; } = false;

    // Todoテンプレートとのリレーション
    public TodoTemplate Template { get; set; } = null!;
}