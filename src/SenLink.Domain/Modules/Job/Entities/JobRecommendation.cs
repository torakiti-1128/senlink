using SenLink.Domain.Common;

namespace SenLink.Domain.Modules.Job.Entities;

/// <summary>
/// 求人レコメンド：教員が特定の学生に求人を推薦する履歴
/// </summary>
public class JobRecommendation : BaseEntity
{
    // 求人ID (FK, NN)
    public long JobId { get; set; }
    
    // 学生ID (NOFK, NN, accounts.id)
    public long StudentAccountId { get; set; }
    
    // 教員ID (NOFK, NN, accounts.id)
    public long RecommenderAccountId { get; set; }

    // 求人とのリレーション
    public Job Job { get; set; } = null!;
}