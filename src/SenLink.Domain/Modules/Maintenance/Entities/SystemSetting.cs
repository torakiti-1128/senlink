using SenLink.Domain.Common;
using SenLink.Domain.Modules.Maintenance.Enums;

namespace SenLink.Domain.Modules.Maintenance.Entities;

/// <summary>
/// システム設定：システムの設定値変更と状態管理
/// </summary>
public class SystemSetting : BaseEntity
{
    // 設定キー (VARCHAR(50), UQ, NN)
    public string Key { get; set; } = null!;

    // 現在の設定値 (TEXT, NN)
    public string Value { get; set; } = null!;

    // データ型 (SMALLINT, NN, Default: 0)
    public SettingValueType ValueType { get; set; } = SettingValueType.String;

    // 説明 (VARCHAR(255))
    public string? Description { get; set; }

    // 更新回数 (INT, NN, Default: 0)
    public int ChangeCounts { get; set; } = 0;

    // 機密設定フラグ (BOOLEAN, NN, Default: false)
    public bool IsSensitive { get; set; } = false;
}