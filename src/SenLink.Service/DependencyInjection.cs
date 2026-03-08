using Microsoft.Extensions.DependencyInjection;
using SenLink.Service.Modules.Auth.Interfaces;
using SenLink.Service.Modules.Auth.Services;

namespace SenLink.Service;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Authモジュールのサービス登録
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
