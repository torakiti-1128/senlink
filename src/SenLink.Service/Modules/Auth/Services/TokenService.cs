using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using SenLink.Domain.Modules.Auth.Entities;
using SenLink.Service.Modules.Auth.Interfaces;

namespace SenLink.Service.Modules.Auth.Services;

/// <summary>
/// JWTトークンを生成するサービス
/// </summary>
/// <param name="config"></param>
public class TokenService(IConfiguration config) : ITokenService
{
    /// <summary>
    /// JWTトークンを生成する
    /// </summary>
    /// <param name="account">アカウント情報</param>
    /// <returns>JWTトークン</returns>
    public string CreateToken(Account account)
    {
        // トークンに含めるクレームを作成
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(ClaimTypes.Email, account.Email),
            new(ClaimTypes.Role, account.Role.ToString())
        };

        // JWTトークンを生成するためのキーと署名情報を作成
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        // トークンの有効期限を設定
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddMinutes(double.Parse(config["JwtSettings:ExpiryMinutes"]!)),
            SigningCredentials = creds,
            Issuer = config["JwtSettings:Issuer"],
            Audience = config["JwtSettings:Audience"]
        };

        // トークンを生成して文字列として返す
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}