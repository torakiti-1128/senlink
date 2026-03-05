using MassTransit;
using SenLink.Domain.Modules.Audit.Contracts;
using SenLink.Domain.Modules.Audit.Entities;

/// <summary>
/// RabbitMQから受信したEventを処理するクラス
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

    /// <summary>
    /// RabbitMQから受信したAuditLogCreatedEventを処理してデータベースに保存する
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Consume(ConsumeContext<AuditLogCreatedEvent> context)
    {
        _logger.LogInformation("Processing AuditLog for {Table}...", context.Message.TargetTable);

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
            CreatedAt = context.Message.CreatedAt,
            UpdatedAt = context.Message.CreatedAt
        };
        
        await _repository.AddAsync(auditLog);
    }
}