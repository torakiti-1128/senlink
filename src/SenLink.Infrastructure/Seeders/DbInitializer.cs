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
        var adminAccount = Account.CreateSystemAdmin("admin@senlink.dev", "AdminPassword123!");

        context.Accounts.Add(adminAccount);
        await context.SaveChangesAsync();
    }
}