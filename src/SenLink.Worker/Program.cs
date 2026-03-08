using SenLink.Infrastructure;
using SenLink.Shared;
using SenLink.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

// Infrastructure 層のサービスを一括登録
builder.Services.AddInfrastructure(builder.Configuration);

// Shared 層の共通設定 (RabbitMQ) を適用
builder.Services.AddSharedMessaging(builder.Configuration, x =>
{
    // このブランチでは Consumer はまだ登録しない、または必要なものだけ登録する
});

var host = builder.Build();
host.Run();
