using System.Text.RegularExpressions;
using SenLink.Domain.Common;
using SenLink.Domain.Modules.Auth.Enums;

namespace SenLink.Domain.Modules.Auth.Entities;

/// <summary>
/// アカウント：認証基盤、ロール管理
/// </summary>
public class Account : BaseEntity
{
    // コンストラクタは private にして、ファクトリメソッドを通じて生成させる
    private Account(string email, AccountRole role)
    {
        Email = email;
        Role = role;
    }

    // メールアドレス (UQ, NN)
    public string Email { get; set; } = string.Empty;

    // パスワード（Hash, NN)
    public string Password { get; set; } = string.Empty;

    // ロール (NN)
    public AccountRole Role { get; set; }

    // 有効フラグ (NN)
    public bool IsActive { get; set; } = true;

    // 論理削除
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// システム管理者アカウントのファクトリメソッド
    /// </summary>
    public static Account CreateSystemAdmin(string email, string plainPassword)
    {
        var account = new Account(email, AccountRole.Admin);
        account.SetPassword(plainPassword);
        return account;
    }

    /// <summary>
    /// アカウント生成のファクトリメソッド（ここでビジネスルールを強制する）
    /// </summary>
    public static Account Create(string email, string plainPassword, string[] allowedDomains)
    {
        // 1. ドメインの検証
        if (!IsValidEmailDomain(email, allowedDomains))
        {
            throw new ArgumentException($"Not allowed domain: {email}");
        }

        // 2. ロールの判定
        var role = DetermineRole(email);

        // 3. インスタンス生成とパスワード設定
        var account = new Account(email, role);
        account.SetPassword(plainPassword);

        return account;
    }

    /// <summary>
    /// メールアドレスが許可されたドメインかどうかを検証する（ドメイン層のビジネスルール）
    /// </summary>
    public static bool IsValidEmailDomain(string email, string[] allowedDomains)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return allowedDomains.Any(domain => email.EndsWith(domain, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// メールアドレスからロールを判定する（ドメインのコアロジック）
    /// </summary>
    private static AccountRole DetermineRole(string email)
    {
        var localPart = email.Split('@')[0];

        // @の前がローマ字（アルファベットとドット等）のみであれば「教師」
        if (Regex.IsMatch(localPart, @"^[a-zA-Z\.]+$"))
        {
            return AccountRole.Teacher;
        }

        // それ以外（数字が含まれる学籍番号など）は「学生」
        return AccountRole.Student;
    }

    /// <summary>
    /// パスワードの設定
    /// </summary>
    /// <param name="rawPassword">平文のパスワード</param>
    public void SetPassword(string rawPassword)
    {
        if (string.IsNullOrWhiteSpace(rawPassword)) throw new ArgumentException("Password cannot be empty.");
        Password = BCrypt.Net.BCrypt.HashPassword(rawPassword);
    }

    /// <summary>
    /// パスワードの検証
    /// </summary>
    /// <param name="rawPassword">平文のパスワード</param>
    /// <returns>パスワードが一致する場合は true、それ以外は false</returns>
    public bool VerifyPassword(string rawPassword)
    {
        return BCrypt.Net.BCrypt.Verify(rawPassword, Password);
    }
}