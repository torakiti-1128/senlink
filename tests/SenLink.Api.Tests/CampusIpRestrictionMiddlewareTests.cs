using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using SenLink.Service.Modules.Maintenance.Interfeces;

namespace SenLink.UnitTests.Api.Middlewares
{
    public class CampusIpRestrictionMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_許可されたIPからのアクセスは次の処理へ進むこと()
        {
            // Arrange
            // 1. Providerのモックを作り、許可IPを返すように設定
            var mockProvider = new Mock<ISystemSettingProvider>();
            mockProvider.Setup(p => p.GetValue("CampusIps")).Returns("192.168.1.100, 10.0.0.5");

            var logger = new NullLogger<CampusIpRestriction>();
            
            // 2. 次の処理(_next)が呼ばれたかどうかのフラグ
            var isNextCalled = false;
            RequestDelegate next = (HttpContext ctx) => 
            {
                isNextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new CampusIpRestriction(next, logger);

            // 3. ダミーのHTTPリクエスト（許可されたIP）を作成
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");

            // Act
            await middleware.InvokeAsync(context, mockProvider.Object);

            // Assert
            Assert.True(isNextCalled, "許可されたIPなので、次の処理（_next）が呼ばれるべき");
        }

        [Fact]
        public async Task InvokeAsync_許可されていないIPからのアクセスは例外を投げること()
        {
            // Arrange
            var mockProvider = new Mock<ISystemSettingProvider>();
            mockProvider.Setup(p => p.GetValue("CampusIps")).Returns("192.168.1.100");

            var logger = new NullLogger<CampusIpRestriction>();
            RequestDelegate next = (HttpContext ctx) => Task.CompletedTask;

            var middleware = new CampusIpRestriction(next, logger);

            // ダミーのHTTPリクエスト（学外の不正なIP）を作成
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.1");

            // Act & Assert
            // 不正なIPなので、ForbiddenException が投げられることを検証
            var exception = await Assert.ThrowsAsync<ForbiddenException>(() => 
                middleware.InvokeAsync(context, mockProvider.Object));
            
            Assert.Equal("学外ネットワークからのアクセスは許可されていません。", exception.Message);
        }
    }
}