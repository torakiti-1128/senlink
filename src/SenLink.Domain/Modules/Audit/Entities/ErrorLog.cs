using SenLink.Domain.Common;
using SenLink.Domain.Modules.Audit.Enums;

namespace SenLink.Domain.Modules.Audit.Entities;

/// <summary>
/// エラーログ：システムエラーの記録
/// </summary>
public class ErrorLog : BaseEntity
{
    // 発生元サービス (VARCHAR(50), NN)
    public string ServiceName { get; set; } = null!;

    // 深刻度 (SMALLINT, NN)
    public ErrorSeverity Severity { get; set; }

    // エラーメッセージの要約 (TEXT, NN)
    public string Message { get; set; } = null!;

    // 詳細なスタックトレース (TEXT)
    public string? StackTrace { get; set; }

    // 発生時のAPIエンドポイントURL (TEXT)
    public string? RequestUrl { get; set; }

    // 発生時のリクエストボディ、クエリ等 (JSONB)
    public RequestParams? RequestParams { get; set; }

    // 発生時にログインしていたアカウントID (NOFK, accounts.id)
    public long? AccountId { get; set; }
}

// リクエストパラメータの構造
public class RequestParams
{
    public Dictionary<string, string>? QueryString { get; set; }
    public string? Body { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
}