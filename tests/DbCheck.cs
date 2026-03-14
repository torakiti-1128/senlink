using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SenLink.Infrastructure.Persistence;

namespace SenLink.Check;

public class DbChecker
{
    public static async Task Main(string[] args)
    {
        var connectionString = "Host=aws-1-ap-southeast-1.pooler.supabase.com;Database=postgres;Username=postgres.qxcdlwkwvwudjglmcxfv;Password=sKv6bd!y!Cj#*M4";
        var optionsBuilder = new DbContextOptionsBuilder<SenLinkDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        using var context = new SenLinkDbContext(optionsBuilder.Options);

        Console.WriteLine("--- AuditLogs (Last 3) ---");
        var auditLogs = await context.AuditLogs
            .OrderByDescending(x => x.CreatedAt)
            .Take(3)
            .ToListAsync();
        foreach (var log in auditLogs)
        {
            Console.WriteLine($"[{log.CreatedAt}] {log.Operation} on {log.TargetTable} (Actor: {log.ActorId})");
        }

        Console.WriteLine("\n--- ErrorLogs (Last 3) ---");
        var errorLogs = await context.ErrorLogs
            .OrderByDescending(x => x.CreatedAt)
            .Take(3)
            .ToListAsync();
        foreach (var log in errorLogs)
        {
            Console.WriteLine($"[{log.CreatedAt}] {log.Severity}: {log.Message} ({log.RequestUrl})");
        }
    }
}
