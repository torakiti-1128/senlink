using SenLink.Domain.Modules.Audit.Entities;
using SenLink.Domain.Modules.Audit.Enums;

namespace SenLink.Domain.Modules.Audit.Contracts;

public record ErrorLogCreatedEvent(
    string ServiceName,
    ErrorSeverity Severity,
    string Message,
    string? StackTrace,
    string? RequestUrl,
    RequestParams? RequestParams,
    long? AccountId
);
