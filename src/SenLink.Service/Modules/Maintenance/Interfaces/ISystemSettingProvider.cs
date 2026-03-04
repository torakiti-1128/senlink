namespace SenLink.Service.Modules.Maintenance.Interfeces
{
    /// <summary>
    /// API層等にシステム設定値を提供する
    /// </summary>
    public interface ISystemSettingProvider
    {
        string? GetValue(string key);
    }
}