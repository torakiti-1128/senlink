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
}

// 学生のプロフィールデータ構造定義
public class StudentProfile
{
    // 自己PR
    public string? Pr { get; set; }

    // 資格・免許
    public string? Certifications { get; set; }

    // Github等のリンク情報
    public string? Links { get; set; }
}