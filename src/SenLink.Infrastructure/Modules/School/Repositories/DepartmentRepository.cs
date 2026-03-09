using Microsoft.EntityFrameworkCore;
using SenLink.Domain.Modules.School.Entities;
using SenLink.Domain.Modules.School.Repositories;
using SenLink.Infrastructure.Persistence;

namespace SenLink.Infrastructure.Modules.School.Repositories;

public class DepartmentRepository(SenLinkDbContext context) : IDepartmentRepository
{
    public async Task<List<Department>> GetAllAsync()
    {
        return await context.Set<Department>().OrderBy(x => x.Id).ToListAsync();
    }

    public async Task<Department?> GetByIdAsync(long id)
    {
        return await context.Set<Department>().FindAsync(id);
    }
}
