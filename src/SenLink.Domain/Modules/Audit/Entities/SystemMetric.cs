using SenLink.Domain.Common;
using SenLink.Domain.Modules.Audit.Enums;

namespace SenLink.Domain.Modules.Audit.Entities;

/// <summary>
/// システムメトリクス：システムリソースの稼働状況
/// </summary>
public class SystemMetric : BaseEntity
{
    // コンポーネント (VARCHAR(50), NN)
    public string Component { get; set; } = null!;

    // ステータス (SMALLINT, NN)
    public ComponentStatus Status { get; set; }

    // 平均レスポンス速度(ms) (INT)
    public int? ResponseTime { get; set; }

    // CPU使用率(%) (NUMERIC(5,2))
    public decimal? CpuUsage { get; set; }

    // メモリ使用率(%) (NUMERIC(5,2))
    public decimal? MemUsage { get; set; }

    // ディスク使用率(%) (NUMERIC(5,2))
    public decimal? DiskUsage { get; set; }
}