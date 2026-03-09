using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using FluentValidation;
using FluentValidation.AspNetCore;
using SenLink.Api.Middlewares;
using SenLink.Infrastructure.Persistence;
using SenLink.Infrastructure.Persistence.Seeders;
using SenLink.Infrastructure;
using SenLink.Service;
using SenLink.Shared;
using SenLink.Api.Filters;
using SenLink.Service.Modules.Maintenance.Services;
using SenLink.Service.Modules.Maintenance.Interfeces;
using SenLink.Domain.Maintenance.Repositories;

// Serilogをセットアップ (アプリ起動前のエラーをキャッチするため)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting SenLink API...");
    var builder = WebApplication.CreateBuilder(args);

    // Serilog を DI コンテナに登録し、appsettings.json の設定を読み込む
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

    // サービス登録
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // バリデーションエラーと成功レスポンスを統一された形式で返すフィルターを登録
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
        options.Filters.Add<SuccessResponseFilter>();
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // ASP.NET Core標準の自動バリデーションレスポンスを抑制し、自作フィルターを優先させる
        options.SuppressModelStateInvalidFilter = true;
    });

    // FluentValidationの有効化（特定のプロジェクトからValidatorをスキャン）
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssembly(typeof(SenLink.Service.DependencyInjection).Assembly);
    builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

    // グローバル例外ハンドラーの登録
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // Infrastructure 層のサービスを一括登録 (DB, Repositories)
    builder.Services.AddInfrastructure(builder.Configuration);

    // Application 層のサービスを一括登録 (Usecases)
    builder.Services.AddApplication();

    // Shared 層の共通設定 (RabbitMQ) を適用
    builder.Services.AddSharedMessaging(builder.Configuration);

    // ヘルスチェックにDBコンテキストの状態を追加
    builder.Services.AddHealthChecks().AddDbContextCheck<SenLinkDbContext>("Database");

    // プロバイダー（キャッシュ）はアプリ全体で1つなので Singleton
    builder.Services.AddSingleton<SystemSettingProvider>(); 
    builder.Services.AddSingleton<ISystemSettingProvider>(sp => sp.GetRequiredService<SystemSettingProvider>());

    // コントローラーでプロバイダーを直接注入できるようにするためのサービス登録
    builder.Services.AddIdentityServices(builder.Configuration);

    // アプリケーションビルド
    var app = builder.Build();

    // Serilog による HTTP リクエストのアクセスログ記録
    app.UseSerilogRequestLogging();

    // リバースプロキシ環境でのクライアントIPとプロトコルの正確な取得
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    // 開発環境でのみSwagger UIを有効化
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        
        // 初期データの流し込み
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<SenLinkDbContext>();
            await DbInitializer.SeedAsync(context);
            Log.Information("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while seeding the database.");
        }
    }

    // Correlation ID をリクエストごとに生成し、ログに付与するミドルウェア
    app.UseMiddleware<CorrelationIdMiddleware>();

    // グローバル例外ハンドラーを有効化
    app.UseExceptionHandler();

    // 学外のIPからのアクセスを制限するミドルウェア
    app.UseMiddleware<CampusIpRestriction>();

    // 認証・認可
    app.UseAuthorization();

    // コントローラーのURLマッピング
    app.MapControllers();

    // ヘルスチェックのエンドポイントを公開
    app.MapHealthChecks("/health");

    // システム設定のキャッシュをロード
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var provider = scope.ServiceProvider.GetRequiredService<SystemSettingProvider>();
            var repository = scope.ServiceProvider.GetRequiredService<ISystemSettingRepository>();
            await provider.LoadCacheAsync(repository);
            Log.Information("System settings cache loaded successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load system settings cache during startup.");
        }
    }

    // アプリケーション起動
    app.Run();
}
catch (Exception ex)
{
    // 起動時エラーが発生した場合
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    // 終了時にバッファに残っているログをすべて書き出す
    Log.CloseAndFlush();
}

public partial class Program { }
