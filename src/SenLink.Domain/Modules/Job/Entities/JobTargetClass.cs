using SenLink.Domain.Common;

namespace SenLink.Domain.Modules.Job.Entities;

/// <summary>
/// 求人公開対象クラス：求人を特定のクラスに限定公開する場合の設定
/// </summary>
public class JobTargetClass : BaseEntity
{
    // 求人ID (FK, NN)
    public long JobId { get; set; }
    
    // クラスID (NOFK, NN, classes.id)
    public long ClassId { get; set; }

    // 求人とのリレーション
    public Job Job { get; set; } = null!;
}