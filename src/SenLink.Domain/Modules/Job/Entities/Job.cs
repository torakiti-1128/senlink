using SenLink.Domain.Common;
using SenLink.Domain.Modules.Job.Enums;

namespace SenLink.Domain.Modules.Job.Entities;

/// <summary>
/// 求人：イベントやインターンなどの募集情報本体
/// </summary>
public class Job : BaseEntity
{
    // 企業ID (FK, NN)
    public long CompanyId { get; set; }

    // ToDoテンプレートID (FK, NN)
    public long TodoTemplateId { get; set; }

    // 教員ID (NOFK, NN, accounts.id)
    public long TeacherAccountId { get; set; }

    // 管理用案件名 (VARCHAR(255), NN)
    public string Title { get; set; } = null!;

    // 種類 (SMALLINT, NN)
    public JobType Type { get; set; }

    // 定員数 (INT)
    public int? Capacity { get; set; }

    // 開催形式 (SMALLINT, NN)
    public JobFormat Format { get; set; }

    // 開催場所・URL (VARCHAR(255))
    public string? Place { get; set; }

    // 緊急連絡先 (VARCHAR(255))
    public string? ContactInfo { get; set; }

    // 開催日／開始日 (DATE)
    public DateOnly? EventStartDate { get; set; }

    // 終了日 (DATE)
    public DateOnly? EventEndDate { get; set; }

    // キャンセル期限日 (DATE)
    public DateOnly? CancelDeadline { get; set; }

    // ステータス (SMALLINT)
    public JobStatus Status { get; set; } = JobStatus.Draft;

    // 公開範囲 (SMALLINT, NN, Default: 0)
    public PublishScope PublishScope { get; set; } = PublishScope.All;

    // 企業紹介／募集要項 (TEXT, NN)
    public string Content { get; set; } = null!;

    // 掲載終了日 (DATE)
    public DateOnly? Deadline { get; set; }

    // 論理削除 (TIMESTAMP)
    public DateTime? DeletedAt { get; set; }

    // 企業、ToDoテンプレートとのリレーション
    public Company Company { get; set; } = null!;
    public TodoTemplate TodoTemplate { get; set; } = null!;
    
    // タグ、推薦、対象クラス・学生、ブックマーク、アンケートなどのリレーション
    public ICollection<JobTag> JobTags { get; set; } = new List<JobTag>();
    public ICollection<JobRecommendation> Recommendations { get; set; } = new List<JobRecommendation>();
    public ICollection<JobTargetClass> TargetClasses { get; set; } = new List<JobTargetClass>();
    public ICollection<JobTargetStudent> TargetStudents { get; set; } = new List<JobTargetStudent>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    public ICollection<Survey> Surveys { get; set; } = new List<Survey>();
}