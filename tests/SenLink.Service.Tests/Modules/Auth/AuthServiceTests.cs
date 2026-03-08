using Moq;
using SenLink.Domain.Modules.Auth.Entities;
using SenLink.Domain.Modules.Auth.Repositories;
using SenLink.Service.Modules.Auth.DTOs;
using SenLink.Service.Modules.Auth.Interfaces;
using SenLink.Service.Modules.Auth.Services;
using SenLink.Service.Modules.Maintenance.Interfeces;
using Xunit;

namespace SenLink.Service.Tests.Modules.Auth;

public class AuthServiceTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IOneTimePasswordRepository> _otpRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<ISystemSettingProvider> _settingProviderMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _otpRepositoryMock = new Mock<IOneTimePasswordRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _settingProviderMock = new Mock<ISystemSettingProvider>();
        
        _authService = new AuthService(
            _accountRepositoryMock.Object,
            _otpRepositoryMock.Object,
            _tokenServiceMock.Object,
            _settingProviderMock.Object
        );
    }

    [Fact]
    public async Task RegisterAsync_ValidDomain_ShouldSucceed()
    {
        var request = new RegisterRequest("student@school.ac.jp", "Password123!");
        _settingProviderMock.Setup(x => x.GetValue("AllowedEmailDomains")).Returns("ac.jp");
        _accountRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync((Account?)null);

        var result = await _authService.RegisterAsync(request);

        Assert.True(result);
        _accountRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Account>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_InvalidDomain_ShouldFail()
    {
        var request = new RegisterRequest("attacker@gmail.com", "Password123!");
        _settingProviderMock.Setup(x => x.GetValue("AllowedEmailDomains")).Returns("ac.jp");

        var result = await _authService.RegisterAsync(request);

        Assert.False(result);
    }

    [Fact]
    public async Task GenerateOtpAsync_ShouldReturn6Digits()
    {
        var otp = await _authService.GenerateOtpAsync("test@school.ac.jp");

        Assert.Equal(6, otp.Length);
        _otpRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OneTimePassword>()), Times.Once);
    }

    [Fact]
    public async Task VerifyOtpAsync_ValidOtp_ShouldReturnTrue()
    {
        var request = new VerifyOtpRequest("test@school.ac.jp", "123456");
        _otpRepositoryMock.Setup(x => x.GetValidOtpAsync(request.Email, request.Otp, "Register"))
            .ReturnsAsync(new OneTimePassword { Email = request.Email, Code = request.Otp });

        var result = await _authService.VerifyOtpAsync(request);

        Assert.True(result);
        _otpRepositoryMock.Verify(x => x.UpdateAsync(It.Is<OneTimePassword>(o => o.IsUsed)), Times.Once);
    }

    [Fact]
    public async Task RequestPasswordResetAsync_ValidEmail_ShouldReturnToken()
    {
        var email = "user@school.ac.jp";
        _accountRepositoryMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(new Account { Email = email });

        var token = await _authService.RequestPasswordResetAsync(email);

        Assert.NotEmpty(token);
        _otpRepositoryMock.Verify(x => x.AddAsync(It.Is<OneTimePassword>(o => o.Purpose == "PasswordReset")), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidToken_ShouldSucceed()
    {
        var request = new ResetPasswordRequest("token123", "NewPass123!");
        var email = "user@school.ac.jp";
        var account = new Account { Email = email };
        
        _otpRepositoryMock.Setup(x => x.GetValidOtpAsync(string.Empty, request.Token, "PasswordReset"))
            .ReturnsAsync(new OneTimePassword { Email = email, Code = request.Token });
        _accountRepositoryMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(account);

        var result = await _authService.ResetPasswordAsync(request);

        Assert.True(result);
        _accountRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Account>()), Times.Once);
    }
}
