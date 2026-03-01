using SenLink.Domain.Common;

namespace SenLink.Domain.Modules.Job.Entities;

/// <summary>
/// 求人タグ中間テーブル：求人とタグを紐づける
/// </summary>
public class JobTag : BaseEntity
{
    // 求人ID (FK, NN)
    public long JobId { get; set; }
    
    // タグID (FK, NN)
    public long TagId { get; set; }

    // 求人とのリレーション
    public Job Job { get; set; } = null!;

    // タグとのリレーション
    public Tag Tag { get; set; } = null!;
}