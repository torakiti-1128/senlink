namespace SenLink.Domain.Modules.Maintenance.Enums;

/// <summary>
/// 設定値のデータ型
/// </summary>
public enum SettingValueType : short
{
    // 文字列
    String = 0,

    // 整数
    Int = 1,

    // 浮動小数点
    Float = 2,

    // 真偽値
    Bool = 3,

    // JSON
    Json = 4
}