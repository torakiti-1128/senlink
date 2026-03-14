using Serilog;
using SenLink.Api.Extensions;

// Serilogをセットアップ (アプリ起動前のエラーをキャッチするため)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting SenLink API...");
    var builder = WebApplication.CreateBuilder(args);

    // サービス登録を外部メソッドに委譲
    builder.ConfigureServices();

    var app = builder.Build();

    // ミドルウェアパイプラインの設定を外部メソッドに委譲
    await app.ConfigurePipelineAsync();

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
