using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SenLink.Domain.Common;
using SenLink.Domain.Modules.Audit.Contracts;
using System.Security.Claims;

namespace SenLink.Infrastructure.Persistence.Interceptors;

/// <summary>
/// データベースの変更内容を取得して RabbitMQ に送信する
/// </summary>
public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditInterceptor(IPublishEndpoint publishEndpoint, IHttpContextAccessor httpContextAccessor)
    {
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <summary>
    /// データベースの変更内容を取得して RabbitMQ に送信する
    /// </summary>
    /// <param name="eventData"></param>
    /// <param name="result"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context == null) return result;

        // データベースの変更内容を取得
        var auditEntries = CaptureChanges(eventData.Context);

        foreach (var entry in auditEntries)
        {
            // RabbitMQ に送信
            await _publishEndpoint.Publish(entry, cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// データベースの変更内容を取得する
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private List<AuditLogCreatedEvent> CaptureChanges(DbContext context)
    {
        // 変更内容を格納するリスト
        var entries = new List<AuditLogCreatedEvent>();

        // ログインしているユーザーのID
        var actorId = GetCurrentUserId();

        // ユーザーのIPアドレス
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        // 変更されたエンティティをループして、変更内容を取得
        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            // 変更がない、または AuditLog 自身ならスキップ
            if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            // 変更内容を組み立てる
            var oldValues = new Dictionary<string, object>();
            var newValues = new Dictionary<string, object>();
            var tableName = entry.Metadata.GetTableName() ?? entry.Metadata.Name;
            var targetId = entry.Property("Id").CurrentValue is long id ? id : 0;
            var method = entry.State.ToString().ToUpper();
            var now = DateTime.UtcNow;

            // 変更されたプロパティをループして、変更内容を取得
            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey()) continue;

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[propertyName] = property.CurrentValue!;
                        break;
                    case EntityState.Deleted:
                        oldValues[propertyName] = property.OriginalValue!;
                        break;
                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            oldValues[propertyName] = property.OriginalValue!;
                            newValues[propertyName] = property.CurrentValue!;
                        }
                        break;
                }
            }

            // ユーザー情報と変更内容をもとにイベントを作成する
            entries.Add(new AuditLogCreatedEvent(
                actorId, tableName, targetId, method, oldValues, newValues, ipAddress, now));
        }

        return entries;
    }

    /// <summary>
    /// 現在のユーザーIDを取得する
    /// </summary>
    /// <returns></returns>
    private long GetCurrentUserId()
    {
        // TODO: HttpContextAccessorへの直接依存を避けるために
        // 抽象化インターフェースを導入して、インフラ層がHTTPに依存しないようにリファクタリングを検討すること
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        return long.TryParse(userIdClaim?.Value, out var id) ? id : 0; // 未ログイン時は0
    }
}