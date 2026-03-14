using Microsoft.Extensions.DependencyInjection;
using SenLink.Service.Common.Interfaces;
using SenLink.Service.Common.Services;
using SenLink.Service.Modules.Auth.Interfaces;
using SenLink.Service.Modules.Auth.Services;
using SenLink.Service.Modules.School.Interfaces;
using SenLink.Service.Modules.School.Services;

namespace SenLink.Service;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // 共通サービス
        services.AddScoped<IEmailSender, DevelopmentEmailSender>();

        // Authモジュールのサービス登録
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISchoolService, SchoolService>();

        return services;
    }
}
