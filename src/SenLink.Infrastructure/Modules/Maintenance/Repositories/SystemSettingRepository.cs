using Microsoft.EntityFrameworkCore;
using SenLink.Domain.Modules.Maintenance.Entities;
using SenLink.Domain.Maintenance.Repositories;
using SenLink.Infrastructure.Persistence;

namespace SenLink.Infrastructure.Modules.Maintenance.Repositories
{
    /// <summary>
    /// システム設定のデータアクセス
    /// </summary>
    public class SystemSettingRepository : ISystemSettingRepository
    {
        private readonly SenLinkDbContext _dbContext;

        public SystemSettingRepository(SenLinkDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// 全てのシステム設定を取得する
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SystemSetting>> GetAllAsync()
        {
            return await _dbContext.SystemSettings.ToListAsync();
        }
    }
}