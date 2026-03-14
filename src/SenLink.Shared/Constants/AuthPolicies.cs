namespace SenLink.Shared.Constants;

/// <summary>
/// 認可ポリシー名の定数定義
/// </summary>
public static class AuthPolicies
{
    /// <summary>
    /// 学生または管理者の権限を要求するポリシー
    /// </summary>
    public const string RequireStudent = "RequireStudent";

    /// <summary>
    /// 教員または管理者の権限を要求するポリシー
    /// </summary>
    public const string RequireTeacher = "RequireTeacher";

    /// <summary>
    /// 管理者の権限を要求するポリシー
    /// </summary>
    public const string RequireAdmin = "RequireAdmin";
}
