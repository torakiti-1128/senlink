using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using SenLink.Domain.Common;
using SenLink.Domain.Modules.Audit.Contracts;
using SenLink.Infrastructure.Persistence;
using SenLink.Infrastructure.Persistence.Interceptors;
using Xunit;

namespace SenLink.Infrastructure.Tests;

public class AuditInterceptorTests
{
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    public AuditInterceptorTests()
    {
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPublishAuditLogCreatedEvent_WhenEntityIsAdded()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<SenLinkDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(new AuditInterceptor(_publishEndpointMock.Object, _httpContextAccessorMock.Object))
            .Options;

        using var context = new SenLinkDbContext(options);

        var department = new Domain.Modules.School.Entities.Department
        {
            Name = "Test Dept",
            Code = "TEST"
        };

        // Act
        context.Departments.Add(department);
        await context.SaveChangesAsync();

        // Assert
        _publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<AuditLogCreatedEvent>(e => 
                    e.TargetTable.Equals("departments", StringComparison.OrdinalIgnoreCase) && 
                    e.Method == "ADDED"),
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPublishAuditLogCreatedEvent_WithOldAndNewValues_WhenEntityIsModified()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<SenLinkDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(new AuditInterceptor(_publishEndpointMock.Object, _httpContextAccessorMock.Object))
            .Options;

        using var context = new SenLinkDbContext(options);
        
        var department = new Domain.Modules.School.Entities.Department { Name = "Old Name", Code = "OLD" };
        context.Departments.Add(department);
        await context.SaveChangesAsync();
        _publishEndpointMock.Invocations.Clear();

        // Act
        department.Name = "New Name";
        await context.SaveChangesAsync();

        // Assert
        _publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<AuditLogCreatedEvent>(e => 
                    e.TargetTable.Equals("departments", StringComparison.OrdinalIgnoreCase) && 
                    e.Method == "MODIFIED" &&
                    e.NewValues.ContainsKey("Name") &&
                    e.NewValues["Name"].ToString() == "New Name" &&
                    e.OldValues["Name"].ToString() == "Old Name"),
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }
}
