using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using SenLink.Api.Filters;
using SenLink.Api.Middlewares;
using SenLink.Domain.Maintenance.Repositories;
using SenLink.Infrastructure;
using SenLink.Infrastructure.Persistence;
using SenLink.Infrastructure.Persistence.Seeders;
using SenLink.Service;
using SenLink.Service.Modules.Maintenance.Interfeces;
using SenLink.Service.Modules.Maintenance.Services;
using SenLink.Shared;
using Serilog;

namespace SenLink.Api.Extensions;

public static class HostingExtensions
{
    /// <summary>
    /// APIプロジェクト固有のサービス登録
    /// </summary>
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        // CORS設定
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", policy =>
            {
                policy.WithOrigins("http://localhost:3000") // フロントエンドのURL
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // Serilog を DI コンテナに登録し、appsettings.json の設定を読み込む
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services));

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
            options.SuppressModelStateInvalidFilter = true;
        });

        // FluentValidationの有効化
        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddValidatorsFromAssembly(typeof(SenLink.Service.DependencyInjection).Assembly);
        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        // 各レイヤーのサービス登録
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddApplication();
        builder.Services.AddSharedMessaging(builder.Configuration);

        // グローバル例外ハンドラーの登録
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        // 共通・ユーティリティサービス
        builder.Services.AddHealthChecks().AddDbContextCheck<SenLinkDbContext>("Database");
        builder.Services.AddSingleton<SystemSettingProvider>();
        builder.Services.AddSingleton<ISystemSettingProvider>(sp => sp.GetRequiredService<SystemSettingProvider>());
        builder.Services.AddIdentityServices(builder.Configuration);

        return builder;
    }

    /// <summary>
    /// ミドルウェアパイプラインと起動時タスクの設定
    /// </summary>
    public static async Task<WebApplication> ConfigurePipelineAsync(this WebApplication app)
    {
        // 監査ログを最優先で記録
        app.UseMiddleware<AuditLogMiddleware>();

        // Serilog によるアクセスログ
        app.UseSerilogRequestLogging();

        // CORSを最優先で有効化（プリフライトリクエストを処理）
        app.UseCors("DefaultPolicy");

        // リバースプロキシ対応
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        // 開発環境固有の設定
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            await app.SeedDatabaseAsync();
        }

        // 基本ミドルウェア
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseExceptionHandler();
        app.UseMiddleware<CampusIpRestriction>();

        // 認証・認可
        app.UseAuthorization();

        // エンドポイント
        app.MapControllers();
        app.MapHealthChecks("/health");

        // 起動時キャッシュロード
        await app.LoadSystemSettingsAsync();

        return app;
    }

    /// <summary>
    /// データベースへ初期データを作成する
    /// </summary>
    private static async Task SeedDatabaseAsync(this WebApplication app)
    {
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

    /// <summary>
    /// 設定マスタからシステム全体の設定値を取得する
    /// </summary>
    private static async Task LoadSystemSettingsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
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
}
