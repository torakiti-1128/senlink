namespace SenLink.Api.Models;

/// <summary>
/// 共通レスポンス形式（基底クラス）
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
}

/// <summary>
/// データを含む共通レスポンス形式
/// </summary>
public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }
}

/// <summary>
/// エラーレスポンス形式
/// </summary>
public class ApiErrorResponse : ApiResponse
{
    public ErrorDetail? Error { get; set; }
}

/// <summary>
/// エラーの詳細情報
/// </summary>
public class ErrorDetail
{
    public string Type { get; set; } = string.Empty;
    public object? Details { get; set; }
}
