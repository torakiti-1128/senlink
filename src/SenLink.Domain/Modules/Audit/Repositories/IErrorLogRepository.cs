using SenLink.Domain.Modules.Audit.Entities;

public interface IErrorLogRepository
{
    Task AddAsync(ErrorLog errorLog);
}