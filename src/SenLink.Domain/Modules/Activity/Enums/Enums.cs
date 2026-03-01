namespace SenLink.Domain.Modules.Activity.Enums;

/// <summary>
/// 就職活動のステータス
/// </summary>
public enum ActivityStatus : short
{
    // 参加前
    BeforeParticipation = 0,

    // 参加済
    Participated = 1,

    // 辞退
    Declined = 2
}

/// <summary>
/// 就職活動のTODOのステータス
/// </summary>
public enum ActivityTodoStatus : short
{
    // 未完了
    Incomplete = 0,

    // 完了
    Completed = 1 
}