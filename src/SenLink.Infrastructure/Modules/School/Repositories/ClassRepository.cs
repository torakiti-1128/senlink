using Microsoft.EntityFrameworkCore;
using SenLink.Domain.Modules.School.Entities;
using SenLink.Domain.Modules.School.Repositories;
using SenLink.Infrastructure.Persistence;

namespace SenLink.Infrastructure.Modules.School.Repositories;

public class ClassRepository(SenLinkDbContext context) : IClassRepository
{
    public async Task<List<Class>> GetFilteredAsync(long? departmentId, int? fiscalYear, int? grade)
    {
        var query = context.Set<Class>()
            .Include(x => x.Department) // 学科情報を結合
            .AsQueryable();

        if (departmentId.HasValue)
            query = query.Where(x => x.DepartmentId == departmentId.Value);

        if (fiscalYear.HasValue)
            query = query.Where(x => x.FiscalYear == fiscalYear.Value);

        if (grade.HasValue)
            query = query.Where(x => x.Grade == grade.Value);

        return await query.OrderByDescending(x => x.FiscalYear).ThenBy(x => x.Grade).ThenBy(x => x.Name).ToListAsync();
    }

    public async Task<Class?> GetByIdAsync(long id)
    {
        return await context.Set<Class>()
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
