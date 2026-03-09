using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SenLink.Api.Models;
using SenLink.Domain.Modules.School.Entities;
using SenLink.Domain.Modules.Auth.Entities;
using SenLink.Domain.Modules.Auth.Enums;
using SenLink.Infrastructure.Persistence;
using SenLink.Service.Modules.School.DTOs;
using Xunit;

namespace SenLink.Api.Tests;

public class SchoolControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _dbName = Guid.NewGuid().ToString();

    public SchoolControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SenLinkDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<SenLinkDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });
            });
        });
        _client = _factory.CreateClient();
    }

    private async Task SeedSchoolDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SenLinkDbContext>();
        
        if (!await context.Departments.AnyAsync())
        {
            context.Departments.Add(new Department { Name = "情報", Code = "INF" });
            context.Departments.Add(new Department { Name = "機械", Code = "MEC" });
            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task GetDepartments_ShouldReturnListOfDepartments()
    {
        // Arrange
        await SeedSchoolDataAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/school/departments");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<DepartmentListResponse>>();
        
        Assert.NotNull(result?.Data);
        Assert.Equal(2, result.Data.Items.Count);
        Assert.Contains(result.Data.Items, d => d.Code == "INF");
    }

    [Fact]
    public async Task GetClasses_ShouldReturnFilteredClasses()
    {
        // Arrange
        await SeedSchoolDataAsync();
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SenLinkDbContext>();
        var dept = await context.Departments.FirstAsync(d => d.Code == "INF");
        
        context.Classes.Add(new Class { DepartmentId = dept.Id, FiscalYear = 2026, Grade = 3, Name = "A組" });
        context.Classes.Add(new Class { DepartmentId = dept.Id, FiscalYear = 2026, Grade = 3, Name = "B組" });
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/school/classes?departmentId={dept.Id}&fiscalYear=2026&grade=3");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClassListResponse>>();
        
        Assert.NotNull(result?.Data);
        Assert.Equal(2, result.Data.Items.Count);
        Assert.All(result.Data.Items, c => Assert.Equal("情報", c.DepartmentName));
    }

    [Fact]
    public async Task CreateStudentProfile_ShouldReturnCreated()
    {
        // Arrange
        await SeedSchoolDataAsync();
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SenLinkDbContext>();
        var dept = await context.Departments.FirstAsync();
        var @class = new Class { DepartmentId = dept.Id, FiscalYear = 2026, Grade = 3, Name = "A組" };
        context.Classes.Add(@class);
        
        // テスト用アカウント
        var account = new Account { Email = "student-test@senlink.dev", Role = AccountRole.Student };
        account.SetPassword("Password123!");
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var request = new {
            classId = @class.Id,
            studentNumber = "1234567",
            name = "山田 太郎",
            nameKana = "やまだ たろう",
            dateOfBirth = "2007-04-01",
            gender = 1,
            admissionYear = 2026
        };

        // TODO: 認証の実装後にトークンをセットする
        _client.DefaultRequestHeaders.Add("X-Account-Id", account.Id.ToString());

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/school/onboarding/student-profile", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetMyStudentProfile_ShouldReturnProfile()
    {
        // Arrange
        await SeedSchoolDataAsync();
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SenLinkDbContext>();
        var dept = await context.Departments.FirstAsync();
        var @class = new Class { DepartmentId = dept.Id, FiscalYear = 2026, Grade = 3, Name = "A組" };
        context.Classes.Add(@class);
        
        var account = new Account { Email = "me-test@senlink.dev", Role = AccountRole.Student };
        account.SetPassword("Password123!");
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        // プロフィールをあらかじめ作成
        var student = new Student {
            AccountId = account.Id,
            ClassId = @class.Id,
            StudentNumber = "9999999",
            Name = "私",
            NameKana = "わたし",
            DateOfBirth = new DateOnly(2000, 1, 1),
            AdmissionYear = 2026
        };
        context.Students.Add(student);
        await context.SaveChangesAsync();

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Account-Id", account.Id.ToString());

        // Act
        var response = await _client.GetAsync("/api/v1/school/students/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<StudentMeResponse>>();
        Assert.NotNull(result?.Data);
        Assert.Equal("9999999", result.Data.StudentNumber);
    }
}
