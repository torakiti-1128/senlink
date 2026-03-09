using SenLink.Domain.Modules.School.Entities;

namespace SenLink.Domain.Modules.School.Repositories;

public interface IStudentRepository
{
    Task AddAsync(Student student);
    Task<Student?> GetByAccountIdAsync(long accountId);
    Task<Student?> GetByStudentNumberAsync(string studentNumber);
    Task UpdateAsync(Student student);
}
