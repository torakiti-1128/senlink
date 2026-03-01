/// <summary>
/// アカウントの権限ロール
/// </summary>
public enum AccountRole : short
{
    // 学生
    Student = 0,

    // 教員
    Teacher = 1,

    // 管理者
    Admin = 2
}

/// <summary>
/// ログイン試行の結果ステータス
/// </summary>
public enum LoginStatus : short
{
    // 失敗
    Failure = 0,

    // 成功
    Success = 1
}