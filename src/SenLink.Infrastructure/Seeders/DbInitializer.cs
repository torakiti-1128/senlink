using SenLink.Domain.Modules.Auth.Entities;
using Microsoft.EntityFrameworkCore;

namespace SenLink.Infrastructure.Persistence.Seeders;

/// <summary>
/// データベースの初期化とシードデータを投入する
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(SenLinkDbContext context)
    {
        // データベースが作成されているか確認
        await context.Database.EnsureCreatedAsync();

        // すでにユーザーがいれば何もしない（二重登録防止）
        if (await context.Accounts.AnyAsync()) return;

        // アドミンユーザーの作成（開発用のためハードコーディング）
        var admin = new Account
        {
            Email = "admin@senlink.dev",
            Role = AccountRole.Admin,
            IsActive = true
        };

        // パスワードをハッシュ化してセット
        admin.SetPassword("AdminPassword123!");

        context.Accounts.Add(admin);
        await context.SaveChangesAsync();
    }
}