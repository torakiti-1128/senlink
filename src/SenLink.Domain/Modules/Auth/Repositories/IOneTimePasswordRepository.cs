using SenLink.Domain.Modules.Auth.Entities;

namespace SenLink.Domain.Modules.Auth.Repositories;

public interface IOneTimePasswordRepository
{
    Task AddAsync(OneTimePassword otp);
    Task<OneTimePassword?> GetValidOtpAsync(string email, string code, string purpose);
    Task<OneTimePassword?> GetValidByTokenAsync(string token, string purpose);
    Task UpdateAsync(OneTimePassword otp);
}
