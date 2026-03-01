namespace SenLink.Domain.Modules.Request.Enums;

/// <summary>
/// 申請の種別
/// </summary>
public enum RequestType : short
{
    // 書類提出
    DocumentSubmission = 0,
    
    // 面接予約
    InterviewReservation = 1,
    
    // 合否発表
    ResultAnnouncement = 2
}

/// <summary>
/// 申請のステータス
/// </summary>
public enum RequestStatus : short
{
    // 下書き
    Draft = 0,
    
    // 申請中
    Submitting = 1,
    
    // 承認
    Approved = 2,
    
    // 差し戻し
    Remanded = 3
}

/// <summary>
/// コメントの種別
/// </summary>
public enum CommentType : short
{
    // メッセージ
    Message = 0,
    
    // 承認
    Approve = 1,
    
    // 差し戻し
    Remand = 2
}

/// <summary>
/// 添付資料のファイル形式
/// </summary>
public enum FileType : short
{
    // 書類
    Document = 0,
    
    // 画像
    Image = 1,
    
    // その他
    Other = 9
}