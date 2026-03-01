using SenLink.Domain.Common;
using SenLink.Domain.Modules.Activity.Enums;

namespace SenLink.Domain.Modules.Activity.Entities;

/// <summary>
/// 就職活動：学生の求人に対する応募状況や活動実績
/// </summary>
public class Activity : BaseEntity
{
    // 求人ID (NOFK, NN, jobs.id)
    public long JobId { get; set; }

    // 学生ID (NOFK, NN, accounts.id)
    public long StudentAccountId { get; set; }

    // ステータス (SMALLINT, NN, Default: 0)
    public ActivityStatus Status { get; set; } = ActivityStatus.BeforeParticipation;

    // 教員ID (NOFK, accounts.id)
    public long? ReviewedByAccountId { get; set; }

    // 就職活動Todoとのリレーション
    public ICollection<ActivityTodo> Todos { get; set; } = new List<ActivityTodo>();
}