using SenLink.Domain.Common;

namespace SenLink.Domain.Modules.Job.Entities;

/// <summary>
/// ToDoテンプレート：求人に関連付けるタスクの雛形
/// </summary>
public class TodoTemplate : BaseEntity
{
    // テンプレート名 (VARCHAR(100), NN)
    public string Name { get; set; } = null!;

    // 説明 (TEXT)
    public string? Description { get; set; }

    // Todoステップとのリレーション
    public ICollection<TodoStep> Steps { get; set; } = new List<TodoStep>();

    // 求人とのリレーション
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}