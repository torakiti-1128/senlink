using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SenLink.Infrastructure.Persistence;
using SenLink.Domain.Modules.Auth.Repositories;
using SenLink.Infrastructure.Modules.Auth.Repositories;
using SenLink.Domain.Maintenance.Repositories;
using SenLink.Infrastructure.Modules.Maintenance.Repositories;

namespace SenLink.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<SenLinkDbContext>(options =>
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly("SenLink.Infrastructure")));

        // Auth
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IOneTimePasswordRepository, OneTimePasswordRepository>();

        // Maintenance
        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();

        return services;
    }
}
