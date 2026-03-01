namespace SenLink.Domain.Modules.Job.Enums;

/// <summary>
/// 求人の種類
/// </summary>
public enum JobType : short
{
    // 説明会
    Briefing = 0,

    // インターン
    Internship = 1,

    // 採用試験
    Exam = 2
}

/// <summary>
/// 開催形式
/// </summary>
public enum JobFormat : short
{
    // 対面
    InPerson = 0,

    // オンライン
    Online = 1,

    // ハイブリッド
    Hybrid = 2
}

/// <summary>
/// 求人のステータス
/// </summary>
public enum JobStatus : short
{
    // 下書き
    Draft = 0,

    // 公開
    Published = 1,

    // 終了
    Closed = 9
}

/// <summary>
/// 公開範囲
/// </summary>
public enum PublishScope : short
{
    // 全体
    All = 0,

    // クラス
    Class = 1,

    // 個別
    Individual = 2
}

/// <summary>
/// タグの種類
/// </summary>
public enum TagType : short
{
    // 職種
    JobCategory = 0,

    // 勤務地
    Location = 1,

    // 特徴
    Feature = 3,

    // 必要なもの
    Requirement = 4
}