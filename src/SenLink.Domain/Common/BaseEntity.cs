namespace SenLink.Domain.Common;

/// <summary>
/// 共通カラムのみを持つエンティティの基底クラス
/// </summary>
public abstract class BaseEntity
{
    // 主キー
    public long Id { get; set; }

    // 作成日時
    public DateTime CreatedAt { get; set; }

    // 更新日時
    public DateTime UpdatedAt { get; set; }
}