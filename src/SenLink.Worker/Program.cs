using SenLink.Infrastructure;
using SenLink.Shared;
using SenLink.Worker;
using SenLink.Worker.Modules.Audit.Consumers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

// Infrastructure 層のサービスを一括登録 (DB, Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// Shared 層の共通設定 (RabbitMQ) を適用し、Worker 特有の Consumer を登録
builder.Services.AddSharedMessaging(builder.Configuration, x =>
{
    x.AddConsumer<AuditLogConsumer>();
    x.AddConsumer<ErrorLogConsumer>();
});

var host = builder.Build();
host.Run();
