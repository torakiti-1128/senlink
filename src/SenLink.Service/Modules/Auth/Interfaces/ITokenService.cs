using SenLink.Domain.Modules.Auth.Entities;

namespace SenLink.Service.Modules.Auth.Interfaces;

/// <summary>
/// JWTトークンを生成するサービスのインターフェース
/// </summary>
public interface ITokenService
{
    string CreateToken(Account account);
}