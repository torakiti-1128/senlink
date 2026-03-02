using SenLink.Api.Models;

namespace SenLink.Api.Middlewares
{
    /// <summary>
    /// カスタム例外の基底クラス
    /// </summary>
    public class SenLinkException : Exception
    {
        public int StatusCode { get; }
        public string ErrorType { get; }
        public List<ValidationErrorDetail>? Errors { get; }

        public SenLinkException(int statusCode, string message, string errorType = "SYSTEM_ERROR", List<ValidationErrorDetail>? errors = null) 
            : base(message)
        {
            StatusCode = statusCode;
            ErrorType = errorType;
            Errors = errors;
        }
    }

    /// <summary>
    /// アクセス権限エラーを表す例外：403 Forbidden
    /// </summary>
    public class ForbiddenException : SenLinkException
    {
        public ForbiddenException(string message = "アクセス権限がありません。") 
            : base(StatusCodes.Status403Forbidden, message, "FORBIDDEN_ERROR") { }
    }

    /// <summary>
    /// バリデーションエラーを表す例外：422 Unprocessable Entity
    /// </summary>
    public class ValidationException : SenLinkException
    {
        public ValidationException(List<ValidationErrorDetail> errors) 
            : base(StatusCodes.Status422UnprocessableEntity, "バリデーションエラーが発生しました。", "VALIDATION_ERROR", errors) { }
    }

    /// <summary>
    /// 認証エラーを表す例外：401 Unauthorized
    /// </summary>
    public class UnauthorizedException : SenLinkException
    {
        public UnauthorizedException(string message = "認証が必要です。") 
            : base(StatusCodes.Status401Unauthorized, message, "UNAUTHORIZED_ERROR") { }
    }

    /// <summary>
    /// リソースが見つからないエラーを表す例外：404 Not Found
    /// </summary>
    public class NotFoundException : SenLinkException
    {
        public NotFoundException(string message = "指定されたリソースが見つかりません。") 
            : base(StatusCodes.Status404NotFound, message, "NOT_FOUND_ERROR") { }
    }

    /// <summary>
    /// データの競合エラーを表す例外：409 Conflict
    /// </summary>
    public class ConflictException : SenLinkException
    {
        public ConflictException(string message = "データが競合しています。") 
            : base(StatusCodes.Status409Conflict, message, "CONFLICT_ERROR") { }
    }

    /// <summary>
    /// 不正なリクエストを表す例外：400 Bad Request
    /// </summary>
    public class BadRequestException : SenLinkException
    {
        public BadRequestException(string message = "不正なリクエストです。") 
            : base(StatusCodes.Status400BadRequest, message, "BAD_REQUEST_ERROR") { }
    }

    /// <summary>
    /// リクエストが多すぎるエラーを表す例外：429 Too Many Requests
    /// </summary>
    public class TooManyRequestsException : SenLinkException
    {
        public TooManyRequestsException(string message = "リクエストが多すぎます。しばらく時間を置いてください。") 
            : base(StatusCodes.Status429TooManyRequests, message, "TOO_MANY_REQUESTS_ERROR") { }
    }
}