using SenLink.Api.Models;

namespace SenLink.Api.Middlewares
{
    /// <summary>
    /// SenLink APIのカスタム例外の基底クラス
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
    /// アクセス権限エラーを表す例外
    /// </summary>
    public class ForbiddenException : SenLinkException
    {
        public ForbiddenException(string message = "アクセス権限がありません。") 
            : base(StatusCodes.Status403Forbidden, message, "FORBIDDEN_ERROR") { }
    }

    /// <summary>
    /// バリデーションエラーを表す例外
    /// </summary>
    public class ValidationException : SenLinkException
    {
        public ValidationException(List<ValidationErrorDetail> errors) 
            : base(StatusCodes.Status422UnprocessableEntity, "バリデーションエラーが発生しました。", "VALIDATION_ERROR", errors) { }
    }
}