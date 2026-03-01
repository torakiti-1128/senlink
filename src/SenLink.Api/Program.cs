using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using SenLink.Infrastructure.Persistence;

// SenLink API アプリケーションのエントリーポイント
var builder = WebApplication.CreateBuilder(args);

// サービス登録
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// DB接続設定
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SenLinkDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        b => b.MigrationsAssembly("SenLink.Infrastructure")
    ));

// アプリケーションビルド
var app = builder.Build();

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

// 認証・認可
app.UseAuthorization();

// コントローラーのURLマッピング
app.MapControllers();

// アプリケーション起動
app.Run();