using Core.Application.Reservations;
using Core.Application.System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Presentation.Shared.LockCleanup;

namespace Presentation.Shared.UnitTests.LockCleanup;

[TestClass]
public class CleanupSchedulerTests
{
    private FetchConfigurationQueryResponse Configuration { get; set; } = null!;
    private Mock<IMediator> MockMediator { get; set; } = null!;
    private CleanupScheduler Subject { get; set; } = null!;

    [TestInitialize]
    public async Task Initialize()
    {
        Configuration = FetchConfigurationQueryResponse.DefaultForTesting;

        MockMediator = new();
        MockMediator
            .Setup(m => m.Send(It.IsAny<FetchConfigurationQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Configuration);

        var mockServiceProvider = CreateMockServiceProvider();
        mockServiceProvider
            .Setup(m => m.GetService(It.Is<Type>(p => p == typeof(IMediator))))
            .Returns((object?)MockMediator.Object);

        Subject = new(mockServiceProvider.Object)
        {
            MaxWaitSeconds = 60,
        };

        await Subject.StartAsync(new CancellationToken());
        await Task.Delay(TimeSpan.FromMilliseconds(250));
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await Subject.StopAsync(new CancellationToken());
    }

    [TestMethod]
    public async Task ScheduleCleanup_WhenExperiationIsX_SchedulesCleanupAfterX()
    {
        // Arrange
        int cleanCount = 1;
        Subject.ProcessingDelaySeconds = 0;
        Configuration.GracePeriodSeconds = 0;
        Configuration.MaxSecondsToConfirmSeat = 1;

        // Act
        await Subject.ScheduleCleanup();
        await Task.Delay(TimeSpan.FromSeconds(Configuration.MaxSecondsToConfirmSeat));
        ++cleanCount;

        // Assert
        MockMediator.Verify(
            m => m.Send(It.IsAny<ClearExpiredLocksCommand>(), It.IsAny<CancellationToken>()),
            Times.Exactly(cleanCount));
    }

    [TestMethod]
    public async Task ScheduleCleanup_WhenNoManualSchedule_SchedulesOnInterval()
    {
        // Arrange
        int cleanCount = 1;
        Subject.MaxWaitSeconds = 2;
        Subject.ProcessingDelaySeconds = 1;
        Configuration.GracePeriodSeconds = 0;
        Configuration.MaxSecondsToConfirmSeat = 0;

        // Cause the config to be reloaded.
        await Subject.ScheduleCleanup();
        await Task.Delay(TimeSpan.FromSeconds(Subject.ProcessingDelaySeconds));
        ++cleanCount;

        // Act
        await Task.Delay(TimeSpan.FromSeconds(Subject.MaxWaitSeconds));
        ++cleanCount;
        await Task.Delay(TimeSpan.FromSeconds(Subject.ProcessingDelaySeconds));

        // Assert
        MockMediator.Verify(
            m => m.Send(It.IsAny<ClearExpiredLocksCommand>(), It.IsAny<CancellationToken>()),
            Times.Exactly(cleanCount));
    }

    private static Mock<IServiceProvider> CreateMockServiceProvider()
    {
        // All this to mock `IServiceProvider`...
        var mockServiceScope = new Mock<IServiceScope>();
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(m => m.GetService(It.Is<Type>(p => p == typeof(IServiceScopeFactory))))
            .Returns(mockServiceScopeFactory.Object);
        mockServiceScope
            .Setup(m => m.ServiceProvider)
            .Returns(mockServiceProvider.Object);
        mockServiceScopeFactory
            .Setup(m => m.CreateScope())
            .Returns(mockServiceScope.Object);
        return mockServiceProvider;
    }
}
