using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SenLink.Shared;

public static class DependencyInjection
{
    /// <summary>
    /// RabbitMQ の共通設定を行う拡張メソッド
    /// </summary>
    public static IServiceCollection AddSharedMessaging(this IServiceCollection services, IConfiguration configuration, Action<IBusRegistrationConfigurator>? configure = null)
    {
        services.AddMassTransit(x =>
        {
            // プロジェクトごとの個別の設定 (Consumerの追加など) を実行
            configure?.Invoke(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqHost = configuration["RabbitMq:Host"] ?? "localhost";
                var rabbitMqUser = configuration["RabbitMq:Username"] ?? "guest";
                var rabbitMqPass = configuration["RabbitMq:Password"] ?? "guest";

                cfg.Host(rabbitMqHost, "/", h =>
                {
                    h.Username(rabbitMqUser);
                    h.Password(rabbitMqPass);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
