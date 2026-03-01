using SenLink.Domain.Common;

namespace SenLink.Domain.Modules.Job.Entities;

/// <summary>
/// 企業：求人情報を出す企業のマスタデータ
/// </summary>
public class Company : BaseEntity
{
    // 企業名 (VARCHAR(255), NN)
    public string Name { get; set; } = null!;

    // 所在地 (VARCHAR(255))
    public string? Address { get; set; }

    // URL (VARCHAR(255))
    public string? Url { get; set; }

    // 求人とのリレーション
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}