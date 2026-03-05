using SenLink.Domain.Modules.Maintenance.Entities;

namespace SenLink.Domain.Maintenance.Repositories
{
    /// <summary>
    /// システム設定のデータアクセス
    /// </summary>
    public interface ISystemSettingRepository
    {
        /// <summary>
        /// 全てのシステム設定を取得する
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<SystemSetting>> GetAllAsync();
    }
}