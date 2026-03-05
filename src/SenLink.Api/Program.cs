using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SenLink.Api.Middlewares;
using SenLink.Infrastructure.Persistence;
using SenLink.Domain.Maintenance.Repositories;
using SenLink.Infrastructure.Modules.Maintenance.Repositories;
using SenLink.Service.Modules.Maintenance.Services;
using SenLink.Service.Modules.Maintenance.Interfeces;

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

    // 成功レスポンスを自動で ApiResponse に包むフィルターを登録
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<SuccessResponseFilter>();
    });

    // グローバル例外ハンドラーの登録
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // DB接続設定
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<SenLinkDbContext>(options =>
        options.UseNpgsql(
            connectionString,
            b => b.MigrationsAssembly("SenLink.Infrastructure")
        ));

    // ヘルスチェックにDBコンテキストの状態を追加
    builder.Services.AddHealthChecks().AddDbContextCheck<SenLinkDbContext>("Database");

    // リポジトリはDBコンテキストを使うため Scoped
    builder.Services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();

    // プロバイダー（キャッシュ）はアプリ全体で1つなので Singleton
    builder.Services.AddSingleton<SystemSettingProvider>(); 
    builder.Services.AddSingleton<ISystemSettingProvider>(sp => sp.GetRequiredService<SystemSettingProvider>());

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