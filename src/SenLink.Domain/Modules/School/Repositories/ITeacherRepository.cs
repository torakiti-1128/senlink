using SenLink.Domain.Modules.School.Entities;

namespace SenLink.Domain.Modules.School.Repositories;

public interface ITeacherRepository
{
    Task AddAsync(Teacher teacher);
    Task<Teacher?> GetByAccountIdAsync(long accountId);
    Task UpdateAsync(Teacher teacher);
}
