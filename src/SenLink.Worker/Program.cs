using MassTransit;
using SenLink.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

// MassTransit の設定 (Consumer側)
builder.Services.AddMassTransit(x =>
{
    // TODO: 次のステップでここに Consumer（受信クラス）を登録します
    // x.AddConsumer<AuditLogConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        var rabbitMqUser = builder.Configuration["RabbitMq:Username"] ?? "guest";
        var rabbitMqPass = builder.Configuration["RabbitMq:Password"] ?? "guest";

        cfg.Host(rabbitMqHost, "/", h =>
        {
            h.Username(rabbitMqUser);
            h.Password(rabbitMqPass);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();
