using Microsoft.EntityFrameworkCore;
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

// 開発環境でのみSwagger UIを有効化
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTP → HTTPS リダイレクト
app.UseHttpsRedirection();

// 認証・認可
app.UseAuthorization();

// コントローラーのURLマッピング
app.MapControllers();

// アプリケーション起動
app.Run();