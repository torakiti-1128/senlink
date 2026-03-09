using Microsoft.EntityFrameworkCore;
using SenLink.Domain.Modules.School.Entities;
using SenLink.Domain.Modules.School.Repositories;
using SenLink.Infrastructure.Persistence;

namespace SenLink.Infrastructure.Modules.School.Repositories;

public class StudentRepository(SenLinkDbContext context) : IStudentRepository
{
    public async Task AddAsync(Student student)
    {
        await context.Set<Student>().AddAsync(student);
        await context.SaveChangesAsync();
    }

    public async Task<Student?> GetByAccountIdAsync(long accountId)
    {
        return await context.Set<Student>()
            .Include(s => s.Class)
            .ThenInclude(c => c.Department)
            .FirstOrDefaultAsync(s => s.AccountId == accountId);
    }

    public async Task<Student?> GetByStudentNumberAsync(string studentNumber)
    {
        return await context.Set<Student>().FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);
    }

    public async Task UpdateAsync(Student student)
    {
        context.Set<Student>().Update(student);
        await context.SaveChangesAsync();
    }
}
