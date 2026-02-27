using SenLink.Domain.Common;

namespace SenLink.Domain.Modules.School.Entities;

/// <summary>
/// 教師：教師の基本情報を持ち、担当クラスとのリレーションを管理する
/// </summary>
public class Teacher : BaseEntity
{
    // アカウントID (FK, NN) - AuthモジュールのAccountIDを参照
    public long AccountId { get; set; } 

    // 教師番号 (VARCHAR(20), NN)
    public string Name { get; set; } = null!;

    // 名前カナ (VARCHAR(50), NN)
    public string NameKana { get; set; } = null!;

    // タイトル (VARCHAR(100), NULL)
    public string? Title { get; set; }

    // オフィスの場所 (VARCHAR(100), NULL)
    public string? OfficeLocation { get; set; }
    
    // 教師のプロフィールデータ(JSONB)
    public TeacherProfile? ProfileData { get; set; }

    // 役割を持っているクラスとのリレーション
    public ICollection<ClassTeacher> ClassTeachers { get; set; } = new List<ClassTeacher>();
}

// 教師のプロフィールデータ構造定義
public class TeacherProfile
{
    // 経歴
    public string? Career { get; set; }

    // 専門分野
    public string? Speciality { get; set; }
}