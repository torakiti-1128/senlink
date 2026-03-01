namespace SenLink.Domain.Modules.School.Enums;

/// <summary>
/// 性別
/// </summary>
public enum Gender : short
{
    // 不明
    Unknown = 0,

    // 男性
    Male = 1,

    // 女性
    Female = 2,

    // その他
    Other = 9
}

/// <summary>
/// 担任の役割
/// </summary>
public enum ClassTeacherRole : short
{
    // 担任
    Homeroom = 0,

    // 副担任
    AssistantHomeroom = 1,

    // 教科担任
    SubjectTeacher = 2,

    // キャリアセンター
    CareerCenter = 3,

    // その他
    Other = 9
}