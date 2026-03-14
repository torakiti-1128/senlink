using SenLink.Domain.Modules.Auth.Entities;
using SenLink.Domain.Modules.Auth.Enums;
using SenLink.Domain.Modules.School.Entities;
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

        // 1. 学科・クラスのシードデータ
        if (!await context.Departments.AnyAsync())
        {
            var departments = new List<Department>
            {
                new() { Name = "情報工学科", Code = "IT" },
                new() { Name = "デザイン学科", Code = "DS" },
                new() { Name = "ビジネス教養学科", Code = "BZ" }
            };
            context.Departments.AddRange(departments);
            await context.SaveChangesAsync();

            var itDept = departments.First(d => d.Code == "IT");
            var currentYear = DateTime.Now.Year;

            var classes = new List<Class>
            {
                new() { DepartmentId = itDept.Id, Name = "情報工学1組", FiscalYear = (short)currentYear, Grade = (short)1 },
                new() { DepartmentId = itDept.Id, Name = "情報工学2組", FiscalYear = (short)currentYear, Grade = (short)1 },
                new() { DepartmentId = itDept.Id, Name = "情報工学プロコース", FiscalYear = (short)currentYear, Grade = (short)2 }
            };
            context.Classes.AddRange(classes);
            await context.SaveChangesAsync();
        }

        // 2. アドミンユーザーの作成
        if (!await context.Accounts.AnyAsync(a => a.Email == "admin@senlink.dev"))
        {
            var adminAccount = Account.CreateSystemAdmin("admin@senlink.dev", "AdminPassword123!");
            context.Accounts.Add(adminAccount);
            await context.SaveChangesAsync();
        }
    }
}
