using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SenLink.Api.Middlewares;
using SenLink.Infrastructure.Persistence;

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

    // グローバル例外ハンドラーを有効化
    app.UseExceptionHandler();

    // 認証・認可
    app.UseAuthorization();

    // コントローラーのURLマッピング
    app.MapControllers();

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