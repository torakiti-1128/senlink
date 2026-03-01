using SenLink.Domain.Common;

namespace SenLink.Domain.Modules.Job.Entities;

/// <summary>
/// ブックマーク：学生が気になった求人を保存する機能
/// </summary>
public class Bookmark : BaseEntity
{
    // 求人ID (FK, NN)
    public long JobId { get; set; }
    
    // 学生ID (NOFK, NN, accounts.id)
    public long StudentAccountId { get; set; }

    // 求人とのリレーション
    public Job Job { get; set; } = null!;
}