using Microsoft.EntityFrameworkCore;
using SenLink.Domain.Modules.Auth.Entities;
using SenLink.Domain.Modules.Auth.Repositories;
using SenLink.Infrastructure.Persistence;

namespace SenLink.Infrastructure.Modules.Auth.Repositories;

public class OneTimePasswordRepository(SenLinkDbContext context) : IOneTimePasswordRepository
{
    public async Task AddAsync(OneTimePassword otp)
    {
        await context.Set<OneTimePassword>().AddAsync(otp);
        await context.SaveChangesAsync();
    }

    public async Task<OneTimePassword?> GetValidOtpAsync(string email, string code, string purpose)
    {
        return await context.Set<OneTimePassword>()
            .FirstOrDefaultAsync(x => x.Email == email && 
                                     x.Code == code && 
                                     x.Purpose == purpose && 
                                     !x.IsUsed && 
                                     x.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<OneTimePassword?> GetValidByTokenAsync(string token, string purpose)
    {
        return await context.Set<OneTimePassword>()
            .FirstOrDefaultAsync(x => x.Code == token && 
                                     x.Purpose == purpose && 
                                     !x.IsUsed && 
                                     x.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<OneTimePassword?> GetAnyByTokenAsync(string token, string purpose)
    {
        return await context.Set<OneTimePassword>()
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(x => x.Code == token && 
                                     x.Purpose == purpose && 
                                     x.ExpiresAt > DateTime.UtcNow);
    }

    public async Task UpdateAsync(OneTimePassword otp)
    {
        context.Set<OneTimePassword>().Update(otp);
        await context.SaveChangesAsync();
    }
}
