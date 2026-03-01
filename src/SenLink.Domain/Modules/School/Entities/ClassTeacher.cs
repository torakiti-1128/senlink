using SenLink.Domain.Common;
using SenLink.Domain.Modules.School.Enums;

namespace SenLink.Domain.Modules.School.Entities;

/// <summary>
/// クラス担任：クラスと教師の多対多リレーションを管理し、担任、副担任などの役割を持たせる
/// </summary>
public class ClassTeacher : BaseEntity
{
    // クラスID (FK, NN)
    public long ClassId { get; set; }

    // 教師ID (FK, NN)
    public long TeacherId { get; set; }

    // 役割 (SMALLINT) 0:担任／1:副担任／2:その他
    public ClassTeacherRole Role { get; set; }

    // クラスとのリレーション
    public Class Class { get; set; } = null!;

    // 教師とのリレーション
    public Teacher Teacher { get; set; } = null!;
}