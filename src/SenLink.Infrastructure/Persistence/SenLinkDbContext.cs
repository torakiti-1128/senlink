using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SenLink.Domain.Modules.Activity.Entities;
using SenLink.Domain.Modules.Auth.Entities;
using SenLink.Domain.Modules.Job.Entities;
using SenLink.Domain.Modules.Request.Entities;
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

    // Job Service Entities
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<TodoTemplate> TodoTemplates => Set<TodoTemplate>();
    public DbSet<TodoStep> TodoSteps => Set<TodoStep>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<JobTag> JobTags => Set<JobTag>();
    public DbSet<JobRecommendation> JobRecommendations => Set<JobRecommendation>();
    public DbSet<JobTargetClass> JobTargetClasses => Set<JobTargetClass>();
    public DbSet<JobTargetStudent> JobTargetStudents => Set<JobTargetStudent>();
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    public DbSet<Survey> Surveys => Set<Survey>();
    public DbSet<SurveyResponse> SurveyResponses => Set<SurveyResponse>();

    // Activity Service Entities
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ActivityTodo> ActivityTodos => Set<ActivityTodo>();

    // Request Service Entities
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<RequestComment> RequestComments => Set<RequestComment>();
    public DbSet<RequestAttachment> RequestAttachments => Set<RequestAttachment>();

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