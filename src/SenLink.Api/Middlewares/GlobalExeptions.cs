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
        public ForbiddenException(string message = "Not allowed to access this resource.") 
            : base(StatusCodes.Status403Forbidden, message, "FORBIDDEN_ERROR") { }
    }

    /// <summary>
    /// バリデーションエラーを表す例外：422 Unprocessable Entity
    /// </summary>
    public class ValidationException : SenLinkException
    {
        public ValidationException(List<ValidationErrorDetail> errors) 
            : base(StatusCodes.Status422UnprocessableEntity, "Validation failed.", "VALIDATION_ERROR", errors) { }
    }

    /// <summary>
    /// 認証エラーを表す例外：401 Unauthorized
    /// </summary>
    public class UnauthorizedException : SenLinkException
    {
        public UnauthorizedException(string message = "Authentication is required.") 
            : base(StatusCodes.Status401Unauthorized, message, "UNAUTHORIZED_ERROR") { }
    }

    /// <summary>
    /// リソースが見つからないエラーを表す例外：404 Not Found
    /// </summary>
    public class NotFoundException : SenLinkException
    {
        public NotFoundException(string message = "The requested resource was not found.") 
            : base(StatusCodes.Status404NotFound, message, "NOT_FOUND_ERROR") { }
    }

    /// <summary>
    /// データの競合エラーを表す例外：409 Conflict
    /// </summary>
    public class ConflictException : SenLinkException
    {
        public ConflictException(string message = "The requested resource conflicts with an existing one.") 
            : base(StatusCodes.Status409Conflict, message, "CONFLICT_ERROR") { }
    }

    /// <summary>
    /// 不正なリクエストを表す例外：400 Bad Request
    /// </summary>
    public class BadRequestException : SenLinkException
    {
        public BadRequestException(string message = "The request is invalid.") 
            : base(StatusCodes.Status400BadRequest, message, "BAD_REQUEST_ERROR") { }
    }

    /// <summary>
    /// リクエストが多すぎるエラーを表す例外：429 Too Many Requests
    /// </summary>
    public class TooManyRequestsException : SenLinkException
    {
        public TooManyRequestsException(string message = "The request is too frequent. Please wait a moment.") 
            : base(StatusCodes.Status429TooManyRequests, message, "TOO_MANY_REQUESTS_ERROR") { }
    }
}