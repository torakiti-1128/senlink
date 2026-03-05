using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SenLink.Domain.Maintenance.Repositories;
using SenLink.Service.Modules.Maintenance.Interfeces;

namespace SenLink.Service.Modules.Maintenance.Services
{
    /// <summary>
    /// アプリケーション全体に設定値のキャッシュを提供するサービス
    /// </summary>
    public class SystemSettingProvider : ISystemSettingProvider
    {
        private readonly ConcurrentDictionary<string, string> _cache = new();
        private readonly ILogger<ISystemSettingProvider> _logger;

        public SystemSettingProvider(ILogger<ISystemSettingProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string? GetValue(string key)
        {
            return _cache.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// 起動時にリポジトリからデータを取得し、キャッシュを構築する
        /// </summary>
        public async Task LoadCacheAsync(ISystemSettingRepository repository)
        {
            try
            {
                var settings = await repository.GetAllAsync();
                
                _cache.Clear();
                foreach (var setting in settings)
                {
                    _cache[setting.Key] = setting.Value;
                }
                
                _logger.LogInformation("Get {Count} settings", _cache.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load system settings cache");
            }
        }
    }
}