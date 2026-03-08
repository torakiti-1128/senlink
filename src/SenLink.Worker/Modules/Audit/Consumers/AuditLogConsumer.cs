using MassTransit;
using SenLink.Domain.Modules.Audit.Contracts;
using SenLink.Domain.Modules.Audit.Entities;
using SenLink.Domain.Modules.Audit.Repositories;

namespace SenLink.Worker.Modules.Audit.Consumers;

/// <summary>
/// データベースの変更通知イベントメッセージを受信して、監査ログを保存する
/// </summary>
public class AuditLogConsumer : IConsumer<AuditLogCreatedEvent>
{
    private readonly IAuditLogRepository _repository;
    private readonly ILogger<AuditLogConsumer> _logger;

    public AuditLogConsumer(ILogger<AuditLogConsumer> logger, IAuditLogRepository repository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task Consume(ConsumeContext<AuditLogCreatedEvent> context)
    {
        _logger.LogInformation("Processing AuditLog for {Table}({Id})...", context.Message.TargetTable, context.Message.TargetId);

        var auditLog = new AuditLog
        {
            ActorId = context.Message.ActorId,
            TargetTable = context.Message.TargetTable,
            TargetId = context.Message.TargetId,
            Method = context.Message.Method,
            Details = new AuditLogDetails
            {
                OldValues = context.Message.OldValues,
                NewValues = context.Message.NewValues
            },
            IpAddress = context.Message.IpAddress,
            CreatedAt = context.Message.CreatedAt
        };

        await _repository.AddAsync(auditLog);
    }
}
