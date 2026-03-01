namespace SenLink.Domain.Modules.Audit.Enums;

/// <summary>
/// エラーの深刻度
/// </summary>
public enum ErrorSeverity : short
{
    // 警告
    Warn = 0,

    // エラー
    Error = 1,

    // 致命的
    Critical = 2
}

/// <summary>
/// システムコンポーネントのステータス
/// </summary>
public enum ComponentStatus : short
{
    // 停止中
    Down = 0,

    // 正常
    Healthy = 1,

    // 高負荷
    HighLoad = 2
}