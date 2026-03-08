using MassTransit;
using SenLink.Domain.Modules.Audit.Contracts;
using SenLink.Domain.Modules.Audit.Entities;

/// <summary>
/// システムエラーが発生したことを通知するイベントメッセージを受信して、エラーログを保存する
/// </summary>
public class ErrorLogConsumer : IConsumer<ErrorLogCreatedEvent>
{
    private readonly IErrorLogRepository _repository;
    private readonly ILogger<ErrorLogConsumer> _logger;
    public ErrorLogConsumer(ILogger<ErrorLogConsumer> logger, IErrorLogRepository repository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task Consume(ConsumeContext<ErrorLogCreatedEvent> context)
    {
        _logger.LogInformation("Processing ErrorLog for {Message}...", context.Message.Message);

        var errorLog = new ErrorLog
        {
            ServiceName = context.Message.ServiceName,
            Severity = context.Message.Severity,
            Message = context.Message.Message,
            StackTrace = context.Message.StackTrace,
            RequestUrl = context.Message.RequestUrl,
            RequestParams = context.Message.RequestParams,
            AccountId = context.Message.AccountId,
            CreatedAt = DateTime.UtcNow
        };
        
        await _repository.AddAsync(errorLog);
    }
}