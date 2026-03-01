using SenLink.Domain.Common;

namespace SenLink.Domain.Modules.Job.Entities;

/// <summary>
/// 求人公開対象学生：求人を特定の学生に限定公開する場合の設定
/// </summary>
public class JobTargetStudent : BaseEntity
{ 
    // 求人ID (FK, NN)
    public long JobId { get; set; }
    
    // 学生アカウントID (NOFK, NN, accounts.id)
    public long StudentAccountId { get; set; }

    // 求人とのリレーション
    public Job Job { get; set; } = null!;
}