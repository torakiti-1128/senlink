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
    // 経歴 (簡易文字列)
    public string? Career { get; set; }

    // 専門分野 (簡易文字列)
    public string? Speciality { get; set; }

    // 構造化された経歴
    public CareerInfo? CareerHistory { get; set; }

    // 専門分野詳細
    public List<SpecialityDetail>? SpecialityDetails { get; set; }

    // 相談に関する案内
    public ConsultationInfo? Consultation { get; set; }

    // 学生へのメッセージ
    public string? Message { get; set; }

    // 外部リンク
    public SocialLinks? SocialLinks { get; set; }
}

public class CareerInfo
{
    public string? Summary { get; set; }
    public List<CareerDetail>? Details { get; set; }
}

public class CareerDetail
{
    public string? Period { get; set; }
    public string? Organization { get; set; }
    public string? Content { get; set; }
}

public class SpecialityDetail
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class ConsultationInfo
{
    public string? Style { get; set; }
    public List<string>? AvailableTopics { get; set; }
    public string? OfficeHours { get; set; }
}