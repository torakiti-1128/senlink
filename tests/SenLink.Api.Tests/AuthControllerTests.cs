using System.Net;
using System.Net.Http.Json;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SenLink.Api.Models;
using SenLink.Domain.Modules.Maintenance.Entities;
using SenLink.Infrastructure.Persistence;
using SenLink.Service.Modules.Auth.DTOs;
using Xunit;

namespace SenLink.Api.Tests;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _dbName = Guid.NewGuid().ToString();

    public AuthControllerTests(WebApplicationFactory<Program> factory)
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

                services.AddMassTransitTestHarness();
            });
        });
        _client = _factory.CreateClient();
    }

    private async Task SeedSettingsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SenLinkDbContext>();
        
        var existing = await context.SystemSettings.FirstOrDefaultAsync(x => x.Key == "AllowedEmailDomains");
        if (existing == null)
        {
            context.SystemSettings.Add(new SystemSetting 
            { 
                Key = "AllowedEmailDomains", 
                Value = "senlink.dev",
                ValueType = SenLink.Domain.Modules.Maintenance.Enums.SettingValueType.String
            });
            await context.SaveChangesAsync();
        }

        // キャッシュをロード（リロード）
        var provider = scope.ServiceProvider.GetRequiredService<SenLink.Service.Modules.Maintenance.Services.SystemSettingProvider>();
        var repository = scope.ServiceProvider.GetRequiredService<SenLink.Domain.Maintenance.Repositories.ISystemSettingRepository>();
        await provider.LoadCacheAsync(repository);
    }

    [Fact]
    public async Task AuthFlow_FullSuccessScenario()
    {
        // Arrange
        await SeedSettingsAsync();

        // 1. Register (senlink.dev)
        var regRequest = new RegisterRequest("test-user@senlink.dev", "SecurePass123!");
        var regResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", regRequest);
        Assert.Equal(HttpStatusCode.OK, regResponse.StatusCode);

        // DBに保存されたか確認
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SenLinkDbContext>();
            var account = await context.Accounts.FirstOrDefaultAsync(a => a.Email == "test-user@senlink.dev");
            Assert.NotNull(account);
        }

        // 2. Login
        var loginRequest = new LoginRequest("test-user@senlink.dev", "SecurePass123!");
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        Assert.NotNull(loginResult?.Data?.Token);

        // 3. OTP Request
        var otpReq = new RequestOtpRequest("test-user@senlink.dev");
        var otpReqResponse = await _client.PostAsJsonAsync("/api/v1/auth/otp/request", otpReq);
        Assert.Equal(HttpStatusCode.OK, otpReqResponse.StatusCode);
        
        var otpResult = await otpReqResponse.Content.ReadFromJsonAsync<ApiResponse<OtpResponse>>();
        Assert.NotNull(otpResult?.Data?.Otp);
        string otpCode = otpResult.Data.Otp;

        // 4. OTP Verify
        var verifyRequest = new VerifyOtpRequest("test-user@senlink.dev", otpCode);
        var verifyResponse = await _client.PostAsJsonAsync("/api/v1/auth/otp/verify", verifyRequest);
        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);

        // 5. Password Reset Request
        var resetReq = new RequestPasswordResetRequest("test-user@senlink.dev");
        var resetReqResponse = await _client.PostAsJsonAsync("/api/v1/auth/password-reset/request", resetReq);
        Assert.Equal(HttpStatusCode.OK, resetReqResponse.StatusCode);
        
        var resetTokenResult = await resetReqResponse.Content.ReadFromJsonAsync<ApiResponse<TokenResponse>>();
        Assert.NotNull(resetTokenResult?.Data?.Token);
        string resetToken = resetTokenResult.Data.Token;

        // 6. Reset Password Execute
        var executeReset = new ResetPasswordRequest(resetToken, "NewStrongPass789!");
        var executeResponse = await _client.PostAsJsonAsync("/api/v1/auth/password-reset/reset", executeReset);
        Assert.Equal(HttpStatusCode.OK, executeResponse.StatusCode);

        // 7. Login with New Password
        var newLoginRequest = new LoginRequest("test-user@senlink.dev", "NewStrongPass789!");
        var newLoginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", newLoginRequest);
        Assert.Equal(HttpStatusCode.OK, newLoginResponse.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidDomain_ShouldReturnBadRequest()
    {
        // Arrange
        await SeedSettingsAsync();
        var request = new RegisterRequest("user@gmail.com", "Password123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldRecordHistory_WhenLoginIsSuccessful()
    {
        // Arrange
        await SeedSettingsAsync();
        var email = "history-test@senlink.dev";
        var regRequest = new RegisterRequest(email, "Password123!");
        await _client.PostAsJsonAsync("/api/v1/auth/register", regRequest);

        var loginRequest = new LoginRequest(email, "Password123!");

        // Act
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Assert: 履歴がDBに作成されているか
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SenLinkDbContext>();
            var account = await context.Accounts.FirstAsync(a => a.Email == email);
            var history = await context.LoginHistories.FirstOrDefaultAsync(h => h.AccountId == account.Id);
            
            Assert.NotNull(history);
            Assert.Equal(SenLink.Domain.Modules.Auth.Enums.LoginStatus.Success, history.Status);
        }
    }

    [Fact]
    public async Task Login_ShouldRecordHistory_WhenLoginFailsDueToWrongPassword()
    {
        // Arrange
        await SeedSettingsAsync();
        var email = "failure-test@senlink.dev";
        var regRequest = new RegisterRequest(email, "Password123!");
        await _client.PostAsJsonAsync("/api/v1/auth/register", regRequest);

        var loginRequest = new LoginRequest(email, "WrongPassword!");

        // Act
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);

        // Assert: 失敗履歴がDBに作成されているか
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SenLinkDbContext>();
            var account = await context.Accounts.FirstAsync(a => a.Email == email);
            var history = await context.LoginHistories.FirstOrDefaultAsync(h => h.AccountId == account.Id);
            
            Assert.NotNull(history);
            Assert.Equal(SenLink.Domain.Modules.Auth.Enums.LoginStatus.Failure, history.Status);
        }
    }

    public class OtpResponse { public string Otp { get; set; } = ""; }
    public class TokenResponse { public string Token { get; set; } = ""; }
}
