using System.Collections.Generic;

namespace SenLink.Api.Models
{
    /// <summary>
    /// APIの成功レスポンスの共通形式
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; set; }
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
    }

    /// <summary>
    /// APIの成功レスポンスの共通形式（データあり）
    /// </summary>
    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }
    }

    /// <summary>
    /// エラー発生時のレスポンス
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
        public string Type { get; set; } = "SERVER_ERROR";
        public List<ValidationErrorDetail>? Details { get; set; }
    }

    /// <summary>
    /// バリデーションエラー
    /// </summary>
    public class ValidationErrorDetail
    {
        public string Field { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}