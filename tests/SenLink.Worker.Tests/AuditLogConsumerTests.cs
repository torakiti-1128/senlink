using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using SenLink.Domain.Modules.Audit.Contracts;
using SenLink.Domain.Modules.Audit.Entities;
using SenLink.Domain.Modules.Audit.Repositories;
using SenLink.Worker.Modules.Audit.Consumers;
using Xunit;

namespace SenLink.Worker.Tests;

public class AuditLogConsumerTests
{
    private readonly Mock<IAuditLogRepository> _repositoryMock;
    private readonly Mock<ILogger<AuditLogConsumer>> _loggerMock;
    private readonly AuditLogConsumer _consumer;

    public AuditLogConsumerTests()
    {
        _repositoryMock = new Mock<IAuditLogRepository>();
        _loggerMock = new Mock<ILogger<AuditLogConsumer>>();
        _consumer = new AuditLogConsumer(_loggerMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task Consume_ShouldCallRepositoryAddAsync()
    {
        // Arrange
        var contextMock = new Mock<ConsumeContext<AuditLogCreatedEvent>>();
        var auditEvent = new AuditLogCreatedEvent(
            ActorId: 1,
            TargetTable: "Departments",
            TargetId: 10,
            Method: "ADDED",
            OldValues: new Dictionary<string, object>(),
            NewValues: new Dictionary<string, object> { { "Name", "Test" } },
            IpAddress: "127.0.0.1",
            CreatedAt: DateTime.UtcNow
        );
        contextMock.Setup(x => x.Message).Returns(auditEvent);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _repositoryMock.Verify(x => x.AddAsync(It.Is<AuditLog>(l => 
            l.ActorId == auditEvent.ActorId &&
            l.TargetTable == auditEvent.TargetTable &&
            l.TargetId == auditEvent.TargetId &&
            l.Method == auditEvent.Method)), Times.Once);
    }
}
