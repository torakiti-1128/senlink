using SenLink.Domain.Common;

namespace SenLink.Domain.Modules.School.Entities;

/// <summary>
/// クラス：学年やクラス名などの基本情報を持ち、学生や担任とのリレーションを管理する
/// </summary>
public class Class : BaseEntity
{
    // 学科ID (FK, NN)
    public long DepartmentId { get; set; }

    // 学年度 (NN)
    public short FiscalYear { get; set; }

    // 学年 (NN)
    public short Grade { get; set; }

    // クラス名 (VARCHAR(50), NN)
    public string Name { get; set; } = null!;

    // 学科とのリレーション
    public Department Department { get; set; } = null!;

    // 学生とのリレーション
    public ICollection<Student> Students { get; set; } = new List<Student>();

    // 役割を持っている教師とのリレーション
    public ICollection<ClassTeacher> ClassTeachers { get; set; } = new List<ClassTeacher>();
}