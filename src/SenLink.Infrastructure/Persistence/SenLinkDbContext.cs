using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SenLink.Domain.Modules.Auth.Entities;
using SenLink.Domain.Modules.School.Entities;

namespace SenLink.Infrastructure.Persistence;

/// <summary>
/// SenLink アプリケーションの EF Core DbContext
/// </summary>
public class SenLinkDbContext : DbContext
{
    public SenLinkDbContext(DbContextOptions<SenLinkDbContext> options)
        : base(options)
    {
    }

    // Auth Service Entities
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<LoginHistory> LoginHistories => Set<LoginHistory>();

    // School Service Entities
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<ClassTeacher> ClassTeachers => Set<ClassTeacher>();

    /// <summary>
    /// モデルを構成する
    /// IEntityTypeConfiguration を実装したクラスを自動で読み込むようにする
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}