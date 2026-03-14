using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SenLink.Service.Modules.Auth.Interfaces;
using SenLink.Service.Modules.Auth.Services;
using SenLink.Shared.Constants;
using System.Text;

/// <summary>
/// JWT認証を設定する拡張メソッド
/// </summary>
public static class IdentityServiceExtensions
{
    /// <summary>
    /// JWT認証を追加する拡張メソッド
    /// </summary>
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<ITokenService, TokenService>();

        // 認可ポリシーの設定
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.RequireStudent, policy => policy.RequireRole("Student", "Admin"));
            options.AddPolicy(AuthPolicies.RequireTeacher, policy => policy.RequireRole("Teacher", "Admin"));
            options.AddPolicy(AuthPolicies.RequireAdmin, policy => policy.RequireRole("Admin"));
        });

        /// JWT認証を追加
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                /// JWTトークンの検証パラメータを設定
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:SecretKey"]!)),
                    ValidateIssuer = true,
                    ValidIssuer = config["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = config["JwtSettings:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }
}
