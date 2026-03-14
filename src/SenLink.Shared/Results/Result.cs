using System.Net;
using SenLink.Shared.Models;

namespace SenLink.Shared.Results;

/// <summary>
/// 汎用的な操作結果を表すクラス
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public HttpStatusCode StatusCode { get; }
    public string ErrorType { get; }
    public List<ValidationErrorDetail>? Errors { get; }

    protected Result(bool isSuccess, string message, HttpStatusCode statusCode, string errorType = "SYSTEM_ERROR", List<ValidationErrorDetail>? errors = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        StatusCode = statusCode;
        ErrorType = errorType;
        Errors = errors;
    }

    // 成功時
    public static Result Success(string message = "Operation successful.") =>
        new(true, message, HttpStatusCode.OK, "NONE");

    // 失敗時
    public static Result Failure(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, string errorType = "BAD_REQUEST_ERROR", List<ValidationErrorDetail>? errors = null) =>
        new(false, message, statusCode, errorType, errors);

    // バリデーションエラー時
    public static Result ValidationFailure(List<ValidationErrorDetail> errors) =>
        new(false, "Validation failed.", HttpStatusCode.UnprocessableEntity, "VALIDATION_ERROR", errors);

    // リソースが見つからない場合
    public static Result NotFound(string message = "The requested resource was not found.") =>
        new(false, message, HttpStatusCode.NotFound, "NOT_FOUND_ERROR");

    // 権限エラー
    public static Result Forbidden(string message = "Not allowed to access this resource.") =>
        new(false, message, HttpStatusCode.Forbidden, "FORBIDDEN_ERROR");
}

/// <summary>
/// 汎用的な操作結果を表すクラス（データ付き）
/// </summary>
/// <typeparam name="T">データの型</typeparam>
public class Result<T> : Result
{
    public T? Data { get; }

    protected Result(bool isSuccess, T? data, string message, HttpStatusCode statusCode, string errorType = "SYSTEM_ERROR", List<ValidationErrorDetail>? errors = null)
        : base(isSuccess, message, statusCode, errorType, errors)
    {
        Data = data;
    }

    // 成功時（データあり）
    public static Result<T> Success(T data, string message = "Operation successful.") =>
        new(true, data, message, HttpStatusCode.OK, "NONE");

    // 失敗時
    public new static Result<T> Failure(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, string errorType = "BAD_REQUEST_ERROR", List<ValidationErrorDetail>? errors = null) =>
        new(false, default, message, statusCode, errorType, errors);

    // リソースが見つからない場合
    public new static Result<T> NotFound(string message = "The requested resource was not found.") =>
        new(false, default, message, HttpStatusCode.NotFound, "NOT_FOUND_ERROR");

    // 権限エラー
    public new static Result<T> Forbidden(string message = "Not allowed to access this resource.") =>
        new(false, default, message, HttpStatusCode.Forbidden, "FORBIDDEN_ERROR");
}
