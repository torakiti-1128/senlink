using SenLink.Domain.Common;
using SenLink.Domain.Modules.Job.Enums;

namespace SenLink.Domain.Modules.Job.Entities;

/// <summary>
/// タグ：求人の特徴や必要なスキルなどのタグマスタ
/// </summary>
public class Tag : BaseEntity
{
    // タグ名 (VARCHAR(50), NN)
    public string Name { get; set; } = null!;
    
    // 種類 (SMALLINT, NN) 0:職種／1:勤務地／3:特徴／4:必要なもの
    public TagType Type { get; set; }

    // JobTag（中間テーブル）とのリレーション
    public ICollection<JobTag> JobTags { get; set; } = new List<JobTag>();
}