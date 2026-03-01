using SenLink.Domain.Common;

namespace SenLink.Domain.Modules.Job.Entities;

/// <summary>
/// アンケート回答：学生からのアンケート回答内容
/// </summary>
public class SurveyResponse : BaseEntity
{
    // アンケート定義ID (FK, NN)
    public long SurveyId { get; set; }
    
    // 学生ID (NOFK, NN, accounts.id)
    public long StudentAccountId { get; set; }

    // 回答内容 (JSONB, NN)
    public SurveyAnswers Answers { get; set; } = null!;

    // アンケート定義とのリレーション
    public Survey Survey { get; set; } = null!;
}

/// <summary>
/// アンケートの回答内容 (JSONB保存用)
/// </summary>
public class SurveyAnswers
{
    // 質問と回答のペア（例: "得意な言語は？" -> "C#"）を保持する辞書
    public Dictionary<string, string> Responses { get; set; } = new Dictionary<string, string>();
}