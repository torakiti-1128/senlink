using SenLink.Domain.Common;
using SenLink.Domain.Modules.Request.Enums;

namespace SenLink.Domain.Modules.Request.Entities;

/// <summary>
/// 添付資料：申請に紐づくファイルデータ
/// </summary>
public class RequestAttachment : BaseEntity
{
    // 申請ID (FK, NN)
    public long RequestId { get; set; }

    // Storage上のパス (VARCHAR(255), NN)
    public string FilePath { get; set; } = null!;

    // ファイル形式 (SMALLINT, NN, Default: 0)
    public FileType FileType { get; set; } = FileType.Document;

    // 補足 (VARCHAR(255))
    public string? Description { get; set; }

    // 申請とのリレーション
    public Request Request { get; set; } = null!;
}