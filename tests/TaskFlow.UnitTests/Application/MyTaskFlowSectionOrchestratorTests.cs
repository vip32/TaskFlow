using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using TaskFlow.Application;
using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;

namespace TaskFlow.UnitTests.Application;

public class MyTaskFlowSectionOrchestratorTests
{
    [Fact]
    public async System.Threading.Tasks.Task GetAllAsync_ForwardsToRepository()
    {
        // Arrange
        var subscription = CreateSubscription();
        var first = new MyTaskFlowSection(subscription.Id, "A", 1);
        var second = new MyTaskFlowSection(subscription.Id, "B", 2);

        var sectionRepository = Substitute.For<IMyTaskFlowSectionRepository>();
        sectionRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns([first, second]);

        var sut = new MyTaskFlowSectionOrchestrator(
            Substitute.For<ILogger<MyTaskFlowSectionOrchestrator>>(),
            sectionRepository,
            Substitute.For<ITaskRepository>(),
            CreateAccessor(subscription));

        // Act
        var result = await sut.GetAllAsync();

        // Assert
        result.Count.ShouldBe(2);
        await sectionRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_UsesCurrentSubscription()
    {
        // Arrange
        var subscription = CreateSubscription();
        var sectionRepository = Substitute.For<IMyTaskFlowSectionRepository>();
        sectionRepository.AddAsync(Arg.Any<MyTaskFlowSection>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<MyTaskFlowSection>());

        var sut = new MyTaskFlowSectionOrchestrator(
            Substitute.For<ILogger<MyTaskFlowSectionOrchestrator>>(),
            sectionRepository,
            Substitute.For<ITaskRepository>(),
            CreateAccessor(subscription));

        // Act
        var created = await sut.CreateAsync("Custom", 3);

        // Assert
        created.SubscriptionId.ShouldBe(subscription.Id);
        await sectionRepository.Received(1).AddAsync(Arg.Is<MyTaskFlowSection>(x => x.SubscriptionId == subscription.Id), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateRuleAsync_UpdatesAndPersists()
    {
        // Arrange
        var subscription = CreateSubscription();
        var section = new MyTaskFlowSection(subscription.Id, "Custom", 1);

        var sectionRepository = Substitute.For<IMyTaskFlowSectionRepository>();
        sectionRepository.GetByIdAsync(section.Id, Arg.Any<CancellationToken>()).Returns(section);
        sectionRepository.UpdateAsync(Arg.Any<MyTaskFlowSection>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<MyTaskFlowSection>());

        var sut = new MyTaskFlowSectionOrchestrator(
            Substitute.For<ILogger<MyTaskFlowSectionOrchestrator>>(),
            sectionRepository,
            Substitute.For<ITaskRepository>(),
            CreateAccessor(subscription));

        // Act
        var updated = await sut.UpdateRuleAsync(section.Id, TaskFlowDueBucket.Upcoming, true, false, false, false);

        // Assert
        updated.DueBucket.ShouldBe(TaskFlowDueBucket.Upcoming);
        updated.IncludeUnassignedTasks.ShouldBeFalse();
        await sectionRepository.Received(1).UpdateAsync(Arg.Any<MyTaskFlowSection>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async System.Threading.Tasks.Task IncludeAndRemoveTaskAsync_PersistsMembership()
    {
        // Arrange
        var subscription = CreateSubscription();
        var section = new MyTaskFlowSection(subscription.Id, "Custom", 1);
        var taskId = Guid.NewGuid();

        var sectionRepository = Substitute.For<IMyTaskFlowSectionRepository>();
        sectionRepository.GetByIdAsync(section.Id, Arg.Any<CancellationToken>()).Returns(section);
        sectionRepository.UpdateAsync(Arg.Any<MyTaskFlowSection>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<MyTaskFlowSection>());

        var sut = new MyTaskFlowSectionOrchestrator(
            Substitute.For<ILogger<MyTaskFlowSectionOrchestrator>>(),
            sectionRepository,
            Substitute.For<ITaskRepository>(),
            CreateAccessor(subscription));

        // Act
        await sut.IncludeTaskAsync(section.Id, taskId);
        var afterInclude = await sut.RemoveTaskAsync(section.Id, taskId);

        // Assert
        section.ManualTasks.ShouldNotContain(x => x.TaskId == taskId);
        afterInclude.ManualTasks.ShouldNotContain(x => x.TaskId == taskId);
        await sectionRepository.Received(2).UpdateAsync(Arg.Any<MyTaskFlowSection>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async System.Threading.Tasks.Task GetSectionTasksAsync_RuleAndManualMembership_ReturnsExpectedTasks()
    {
        // Arrange
        var subscription = CreateSubscription();
        var todaySection = MyTaskFlowSection.CreateSystem(subscription.Id, "Today", 1, TaskFlowDueBucket.Today);

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(subscription.TimeZoneId);
        var todayLocal = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));

        var dueTask = new DomainTask(subscription.Id, "Due", Guid.NewGuid());
        dueTask.SetDueDate(todayLocal);

        var manualTask = new DomainTask(subscription.Id, "Manual", null);
        todaySection.IncludeTask(manualTask.Id);

        var sectionRepository = Substitute.For<IMyTaskFlowSectionRepository>();
        sectionRepository.GetByIdAsync(todaySection.Id, Arg.Any<CancellationToken>()).Returns(todaySection);

        var taskRepository = Substitute.For<ITaskRepository>();
        taskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns([dueTask, manualTask]);

        var sut = new MyTaskFlowSectionOrchestrator(
            Substitute.For<ILogger<MyTaskFlowSectionOrchestrator>>(),
            sectionRepository,
            taskRepository,
            CreateAccessor(subscription));

        // Act
        var tasks = await sut.GetSectionTasksAsync(todaySection.Id);

        // Assert
        tasks.Count.ShouldBe(2);
        tasks.ShouldContain(task => task.Id == dueTask.Id);
        tasks.ShouldContain(task => task.Id == manualTask.Id);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetSectionTasksAsync_ImportantSection_ReturnsOnlyImportantTasks()
    {
        // Arrange
        var subscription = CreateSubscription();
        var importantSection = MyTaskFlowSection.CreateSystem(subscription.Id, "Important", 2, TaskFlowDueBucket.Important);

        var starredTask = new DomainTask(subscription.Id, "Starred", Guid.NewGuid());
        starredTask.ToggleImportant();
        var regularTask = new DomainTask(subscription.Id, "Regular", Guid.NewGuid());

        var taskRepository = Substitute.For<ITaskRepository>();
        taskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns([starredTask, regularTask]);

        var sut = new MyTaskFlowSectionOrchestrator(
            Substitute.For<ILogger<MyTaskFlowSectionOrchestrator>>(),
            Substitute.For<IMyTaskFlowSectionRepository>(),
            taskRepository,
            CreateAccessor(subscription));

        // Act
        var tasks = await sut.GetSectionTasksAsync(importantSection);

        // Assert
        tasks.Count.ShouldBe(1);
        tasks[0].Id.ShouldBe(starredTask.Id);
    }

    private static Subscription CreateSubscription()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true, "Europe/Berlin");
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));
        return subscription;
    }

    private static ICurrentSubscriptionAccessor CreateAccessor(Subscription subscription)
    {
        var accessor = Substitute.For<ICurrentSubscriptionAccessor>();
        accessor.GetCurrentSubscription().Returns(subscription);
        return accessor;
    }
}
