namespace SenLink.Shared.Models;

/// <summary>
/// バリデーションエラーの詳細（Shared）
/// </summary>
public class ValidationErrorDetail
{
    public string Field { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
