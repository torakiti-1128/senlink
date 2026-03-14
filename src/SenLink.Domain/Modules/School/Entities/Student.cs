using SenLink.Domain.Common;
using SenLink.Domain.Modules.School.Enums;

namespace SenLink.Domain.Modules.School.Entities;

/// <summary>
/// 学生：学生の基本情報を持ち、クラスやプロフィールデータとのリレーションを管理する
/// </summary>
public class Student : BaseEntity
{
    // アカウントID (FK, NN) - AuthモジュールのAccountIDを参照
    public long AccountId { get; set; }

    // クラスID (FK, NN)
    public long ClassId { get; set; }

    // 学生番号 (VARCHAR(20), NN)
    public string StudentNumber { get; set; } = null!;

    // 名前 (VARCHAR(100), NN)
    public string Name { get; set; } = null!;

    // 名前カナ (VARCHAR(50), NN)
    public string NameKana { get; set; } = null!;

    // 生年月日 (DATE, NN)
    public DateOnly DateOfBirth { get; set; }

    // 性別 (SMALLINT) 
    public Gender Gender { get; set; } = Gender.Unknown;

    // 入学年度 (NN)
    public int AdmissionYear { get; set; }

    // 就職活動状況 (BOOLEAN) true:就活中／false:就活終了
    public bool IsJobHunting { get; set; } = true;

    // 学生のプロフィールデータ (JSONB)
    public StudentProfile? ProfileData { get; set; }

    // クラスとのリレーション
    public Class Class { get; set; } = null!;

    /// <summary>
    /// 学生プロフィールのドメインルールを検証します
    /// </summary>
    /// <param name="accountEmail">ログイン中のメールアドレス</param>
    public void Validate(string accountEmail)
    {
        // 1. 学生番号は8桁
        if (StudentNumber.Length != 8 || !StudentNumber.All(char.IsDigit))
        {
            throw new ArgumentException("Student number must be exactly 8 digits.");
        }

        // 2. メールアドレスのユーザー名と学生番号が一致すること
        var emailUsername = accountEmail.Split('@')[0];
        if (emailUsername != StudentNumber)
        {
            throw new ArgumentException("Student number must match the email username.");
        }
    }
}

// 学生のプロフィールデータ構造定義
public class StudentProfile
{
    // 自己PR (基本)
    public string? Pr { get; set; }

    // 資格・免許 (簡易文字列)
    public string? Certifications { get; set; }

    // Github等のリンク情報 (簡易文字列)
    public string? Links { get; set; }

    // 学歴
    public List<AcademicHistory>? AcademicHistories { get; set; }

    // 職歴・インターン
    public List<WorkHistory>? WorkHistories { get; set; }

    // 取得資格詳細 (日付等を含む構造化データ)
    public List<CertificationDetail>? CertificationDetails { get; set; }

    // スキルセット
    public SkillSet? Skills { get; set; }

    // 外部リンク詳細
    public SocialLinks? SocialLinks { get; set; }

    // 自己PR詳細
    public SelfPromotionDetail? SelfPromotion { get; set; }
}

public class AcademicHistory
{
    public string SchoolName { get; set; } = null!;
    public string? Faculty { get; set; }
    public string StartDate { get; set; } = null!; // YYYY-MM
    public string? EndDate { get; set; }   // YYYY-MM
    public string Status { get; set; } = "Graduated"; // Graduated, Expected, Dropout, etc.
}

public class WorkHistory
{
    public string Type { get; set; } = "Internship"; // Internship, PartTime, Volunteer
    public string Organization { get; set; } = null!;
    public string? Role { get; set; }
    public string? Content { get; set; }
    public string StartDate { get; set; } = null!;
    public string? EndDate { get; set; }
}

public class CertificationDetail
{
    public string Name { get; set; } = null!;
    public string? Date { get; set; } // YYYY-MM
}

public class SkillSet
{
    public List<string>? Languages { get; set; }
    public List<string>? Frameworks { get; set; }
    public List<string>? Others { get; set; }
}

public class SocialLinks
{
    public string? Github { get; set; }
    public string? Portfolio { get; set; }
    public string? Blog { get; set; }
    public string? Twitter { get; set; }
}

public class SelfPromotionDetail
{
    public string? Catchphrase { get; set; }
    public string? Content { get; set; }
    public List<string>? Strengths { get; set; }
}