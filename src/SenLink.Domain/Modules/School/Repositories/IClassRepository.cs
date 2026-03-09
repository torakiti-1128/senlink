using SenLink.Domain.Modules.School.Entities;

namespace SenLink.Domain.Modules.School.Repositories;

public interface IClassRepository
{
    Task<List<Class>> GetFilteredAsync(long? departmentId, int? fiscalYear, int? grade);
    Task<Class?> GetByIdAsync(long id);
}
