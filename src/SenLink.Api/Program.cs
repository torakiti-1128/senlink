using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using FluentValidation;
using FluentValidation.AspNetCore;
using SenLink.Api.Middlewares;
using SenLink.Infrastructure.Persistence;
using SenLink.Infrastructure.Persistence.Seeders;
using SenLink.Infrastructure;
using SenLink.Shared;
using SenLink.Api.Filters;
using SenLink.Service.Modules.Maintenance.Services;
using SenLink.Service.Modules.Maintenance.Interfeces;
using SenLink.Service.Modules.Auth.Interfaces;
using SenLink.Service.Modules.Auth.Services;
using MassTransit;

// Serilogをセットアップ
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting SenLink API...");
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<SuccessResponseFilter>();
    });

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // Infrastructure 層のサービスを一括登録 (DB, Repositories)
    builder.Services.AddInfrastructure(builder.Configuration);

    // Shared 層の共通設定 (RabbitMQ) を適用
    builder.Services.AddSharedMessaging(builder.Configuration);

    builder.Services.AddHealthChecks().AddDbContextCheck<SenLinkDbContext>("Database");

    builder.Services.AddSingleton<SystemSettingProvider>(); 
    builder.Services.AddSingleton<ISystemSettingProvider>(sp => sp.GetRequiredService<SystemSettingProvider>());

    // Auth & Token
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IAuthService, AuthService>();

    builder.Services.AddIdentityServices(builder.Configuration);

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        
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

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseExceptionHandler();
    app.UseMiddleware<CampusIpRestriction>();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
