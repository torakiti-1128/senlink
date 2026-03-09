using Microsoft.EntityFrameworkCore;
using SenLink.Domain.Modules.School.Entities;
using SenLink.Domain.Modules.School.Repositories;
using SenLink.Infrastructure.Persistence;

namespace SenLink.Infrastructure.Modules.School.Repositories;

public class TeacherRepository(SenLinkDbContext context) : ITeacherRepository
{
    public async Task AddAsync(Teacher teacher)
    {
        await context.Set<Teacher>().AddAsync(teacher);
        await context.SaveChangesAsync();
    }

    public async Task<Teacher?> GetByAccountIdAsync(long accountId)
    {
        return await context.Set<Teacher>().FirstOrDefaultAsync(t => t.AccountId == accountId);
    }

    public async Task UpdateAsync(Teacher teacher)
    {
        context.Set<Teacher>().Update(teacher);
        await context.SaveChangesAsync();
    }
}
