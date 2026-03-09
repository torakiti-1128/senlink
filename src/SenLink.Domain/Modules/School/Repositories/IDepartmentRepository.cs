using SenLink.Domain.Modules.School.Entities;

namespace SenLink.Domain.Modules.School.Repositories;

public interface IDepartmentRepository
{
    Task<List<Department>> GetAllAsync();
    Task<Department?> GetByIdAsync(long id);
}
