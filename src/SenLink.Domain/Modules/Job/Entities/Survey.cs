using SenLink.Domain.Common;

namespace SenLink.Domain.Modules.Job.Entities;

/// <summary>
/// アンケート定義：求人ごとに設定できるアンケートの定義
/// </summary>
public class Survey : BaseEntity
{
    // 求人ID (FK, NN)
    public long JobId { get; set; }
    
    // アンケート名 (VARCHAR(255), NN)
    public string Title { get; set; } = null!;

    // 質問項目 (JSONB, NN)
    public SurveyQuestions Questions { get; set; } = null!;

    // 求人とのリレーション
    public Job Job { get; set; } = null!;

    // アンケート回答とのリレーション
    public ICollection<SurveyResponse> Responses { get; set; } = new List<SurveyResponse>();
}

/// <summary>
/// アンケートの質問項目 (JSONB保存用)
/// </summary>
public class SurveyQuestions
{
    public List<QuestionItem> Items { get; set; } = new List<QuestionItem>();
}

/// <summary>
/// アンケート回答：学生がアンケートに回答した内容を保存する
/// </summary>
public class QuestionItem
{
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new List<string>();
}