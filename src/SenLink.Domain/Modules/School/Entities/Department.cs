using SenLink.Domain.Common;

namespace SenLink.Domain.Modules.School.Entities;

/// <summary>
/// 学科：学部や専攻などの大分類を表し、クラスや学生の所属を管理する
/// </summary>
public class Department : BaseEntity
{
    // 学科名 (VARCHAR(100), NN)
    public string Name { get; set; } = null!;

    // 学科コード (VARCHAR(20), NN)
    public string Code { get; set; } = null!;

    // クラスとのリレーション
    public ICollection<Class> Classes { get; set; } = new List<Class>();
}