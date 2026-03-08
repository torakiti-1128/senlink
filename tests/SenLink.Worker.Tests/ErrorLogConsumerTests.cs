using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using SenLink.Domain.Modules.Audit.Contracts;
using SenLink.Domain.Modules.Audit.Entities;
using SenLink.Domain.Modules.Audit.Enums;
using SenLink.Domain.Modules.Audit.Repositories;
using SenLink.Worker.Modules.Audit.Consumers;
using Xunit;

namespace SenLink.Worker.Tests;

public class ErrorLogConsumerTests
{
    private readonly Mock<IErrorLogRepository> _repositoryMock;
    private readonly Mock<ILogger<ErrorLogConsumer>> _loggerMock;
    private readonly ErrorLogConsumer _consumer;

    public ErrorLogConsumerTests()
    {
        _repositoryMock = new Mock<IErrorLogRepository>();
        _loggerMock = new Mock<ILogger<ErrorLogConsumer>>();
        _consumer = new ErrorLogConsumer(_loggerMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task Consume_ShouldCallRepositoryAddAsync()
    {
        // Arrange
        var contextMock = new Mock<ConsumeContext<ErrorLogCreatedEvent>>();
        var errorEvent = new ErrorLogCreatedEvent(
            ServiceName: "TestService",
            Severity: ErrorSeverity.Error,
            Message: "Test Exception",
            StackTrace: "Test StackTrace",
            RequestUrl: "GET /api/test",
            RequestParams: new RequestParams { QueryString = new Dictionary<string, string> { { "id", "1" } } },
            AccountId: 123
        );
        contextMock.Setup(x => x.Message).Returns(errorEvent);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _repositoryMock.Verify(x => x.AddAsync(It.Is<ErrorLog>(l => 
            l.ServiceName == errorEvent.ServiceName &&
            l.Message == errorEvent.Message &&
            l.Severity == errorEvent.Severity &&
            l.AccountId == errorEvent.AccountId)), Times.Once);
    }
}
