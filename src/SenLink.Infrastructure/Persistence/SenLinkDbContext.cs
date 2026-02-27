using Microsoft.EntityFrameworkCore;
using SenLink.Domain.Modules.Auth.Entities;

namespace SenLink.Infrastructure.Persistence;

/// <summary>
/// SenLink アプリケーションの EF Core DbContext
/// </summary>
public class SenLinkDbContext : DbContext
{
    public SenLinkDbContext(DbContextOptions<SenLinkDbContext> options) : base(options) { }

    // 各モジュールのDbSet
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<LoginHistory> LoginHistories => Set<LoginHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Infrastructure 内の全 IEntityTypeConfiguration を自動適用
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SenLinkDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}