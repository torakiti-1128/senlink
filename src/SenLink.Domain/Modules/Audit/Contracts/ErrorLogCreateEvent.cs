namespace SenLink.Domain.Modules.Audit.Contracts;

/// <summary>
/// システムエラーが発生したことを通知するイベントメッセージ
/// </summary>
public record ErrorLogCreatedEvent(
    string ServiceName,
    short Severity,
    string Message,
    string? StackTrace,
    string? RequestUrl,
    string? RequestParams,
    long? AccountId
);