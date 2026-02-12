using Microsoft.Extensions.Logging.Abstractions;
using TaskFlow.Application;
using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;
using DomainTaskStatus = TaskFlow.Domain.TaskStatus;

namespace TaskFlow.UnitTests.Application;

[Trait("Layer", "Application")]
[Trait("Slice", "Tasks")]
[Trait("Type", "Unit")]
public class TaskOrchestratorTests
{
    [Fact]
    public async System.Threading.Tasks.Task GetByProjectIdAsync_ForwardsToRepository()
    {
        // Arrange
        var subscription = CreateSubscription();
        var projectId = Guid.NewGuid();
        var existing = new DomainTask(subscription.Id, "A", projectId);

        var repository = new FakeTaskRepository(existing);

        // Act
        var sut = CreateSut(subscription, repository, new FakeTaskHistoryRepository());

        var result = await sut.GetByProjectIdAsync(projectId);

        // Assert
        result.ShouldHaveSingleItem();
    }

    [Fact]
    public async System.Threading.Tasks.Task GetNameSuggestionsAsync_ForwardsToHistoryRepository()
    {
        // Arrange
        var subscription = CreateSubscription();
        var history = new FakeTaskHistoryRepository
        {
            Suggestions = ["Plan", "Planning"],
        };

        // Act
        var sut = CreateSut(subscription, new FakeTaskRepository(), history);
        var result = await sut.GetNameSuggestionsAsync("Pl", false, 5);

        // Assert
        result.Count.ShouldBe(2);
        history.LastPrefix.ShouldBe("Pl");
        history.LastIsSubTaskName.ShouldBeFalse();
        history.LastTake.ShouldBe(5);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_ValidInput_PersistsTaskAndRegistersHistory()
    {
        // Arrange
        var subscription = CreateSubscription();
        var history = new FakeTaskHistoryRepository();
        var repository = new FakeTaskRepository();

        // Act
        var sut = CreateSut(subscription, repository, history);

        var created = await sut.CreateAsync(Guid.NewGuid(), "Draft roadmap", TaskPriority.High, "Initial note");

        // Assert
        created.SubscriptionId.ShouldBe(subscription.Id);
        created.Priority.ShouldBe(TaskPriority.High);
        repository.AddCallCount.ShouldBe(1);
        history.Registered.ShouldHaveSingleItem();
        history.Registered[0].IsSubTaskName.ShouldBeFalse();
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateUnassignedAsync_ValidInput_PersistsUnassignedTask()
    {
        // Arrange
        var subscription = CreateSubscription();
        var repository = new FakeTaskRepository();

        // Act
        var sut = CreateSut(subscription, repository, new FakeTaskHistoryRepository());

        var created = await sut.CreateUnassignedAsync("Inbox idea", TaskPriority.Medium, string.Empty);

        // Assert
        created.ProjectId.ShouldBeNull();
        created.IsUnassigned.ShouldBeTrue();
        repository.AddCallCount.ShouldBe(1);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateSubTaskAsync_ValidInput_InheritsParentProjectAndRegistersSubtaskHistory()
    {
        // Arrange
        var subscription = CreateSubscription();
        var projectId = Guid.NewGuid();
        var parent = new DomainTask(subscription.Id, "Parent", projectId);
        var repository = new FakeTaskRepository(parent);
        var history = new FakeTaskHistoryRepository();

        // Act
        var sut = CreateSut(subscription, repository, history);

        var created = await sut.CreateSubTaskAsync(parent.Id, "Child", TaskPriority.Low, "n");

        // Assert
        created.ParentTaskId.ShouldBe(parent.Id);
        created.ProjectId.ShouldBe(projectId);
        history.Registered[0].IsSubTaskName.ShouldBeTrue();
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTitleAsync_ExistingTask_PersistsChangeAndRegistersHistory()
    {
        // Arrange
        var subscription = CreateSubscription();
        var existing = new DomainTask(subscription.Id, "Old", Guid.NewGuid());
        var history = new FakeTaskHistoryRepository();
        var repository = new FakeTaskRepository(existing);

        // Act
        var sut = CreateSut(subscription, repository, history);

        var updated = await sut.UpdateTitleAsync(existing.Id, "New");

        // Assert
        updated.Title.ShouldBe("New");
        repository.UpdateCallCount.ShouldBe(1);
        history.Registered.ShouldHaveSingleItem();
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateNoteSetPrioritySetStatus_ToggleFlags_PersistChanges()
    {
        // Arrange
        var subscription = CreateSubscription();
        var existing = new DomainTask(subscription.Id, "Task", Guid.NewGuid());
        var repository = new FakeTaskRepository(existing);

        // Act
        var sut = CreateSut(subscription, repository, new FakeTaskHistoryRepository());

        await sut.UpdateNoteAsync(existing.Id, "note");
        await sut.SetPriorityAsync(existing.Id, TaskPriority.Low);
        await sut.SetStatusAsync(existing.Id, DomainTaskStatus.Doing);
        await sut.ToggleFocusAsync(existing.Id);

        var important = await sut.ToggleImportantAsync(existing.Id);

        // Assert
        important.Note.ShouldBe("note");
        important.Priority.ShouldBe(TaskPriority.Low);
        important.Status.ShouldBe(DomainTaskStatus.Doing);
        important.IsFocused.ShouldBeTrue();
        important.IsImportant.ShouldBeTrue();
    }

    [Fact]
    public async System.Threading.Tasks.Task MoveToProjectAsync_SubTask_ThrowsInvalidOperationException()
    {
        // Arrange
        var subscription = CreateSubscription();
        var parent = new DomainTask(subscription.Id, "Parent", Guid.NewGuid());
        var child = new DomainTask(subscription.Id, "Child", parent.ProjectId);
        parent.AddSubTask(child);

        var repository = new FakeTaskRepository(parent, child);

        // Act
        var sut = CreateSut(subscription, repository, new FakeTaskHistoryRepository());

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(() => sut.MoveToProjectAsync(child.Id, Guid.NewGuid()));
    }

    [Fact]
    public async System.Threading.Tasks.Task MoveToProjectAsync_TopLevelTask_AssignsProjectAndSortOrder()
    {
        // Arrange
        var subscription = CreateSubscription();
        var existingInTarget = new DomainTask(subscription.Id, "Existing", Guid.NewGuid());
        existingInTarget.SetSortOrder(3);
        var moving = new DomainTask(subscription.Id, "Move", Guid.NewGuid());

        var repository = new FakeTaskRepository(existingInTarget, moving);

        // Act
        var sut = CreateSut(subscription, repository, new FakeTaskHistoryRepository());

        var moved = await sut.MoveToProjectAsync(moving.Id, existingInTarget.ProjectId!.Value);

        // Assert
        moved.ProjectId.ShouldBe(existingInTarget.ProjectId);
        moved.SortOrder.ShouldBe(4);
    }

    [Fact]
    public async System.Threading.Tasks.Task ReorderProjectTasksAsync_ValidOrder_PersistsSortOrder()
    {
        // Arrange
        var subscription = CreateSubscription();
        var projectId = Guid.NewGuid();

        var first = new DomainTask(subscription.Id, "First", projectId);
        var second = new DomainTask(subscription.Id, "Second", projectId);
        first.SetSortOrder(0);
        second.SetSortOrder(1);

        var repository = new FakeTaskRepository(first, second);

        // Act
        var sut = CreateSut(subscription, repository, new FakeTaskHistoryRepository());

        var reordered = await sut.ReorderProjectTasksAsync(projectId, [second.Id, first.Id]);

        // Assert
        reordered[0].Id.ShouldBe(second.Id);
        reordered[0].SortOrder.ShouldBe(0);
        reordered[1].SortOrder.ShouldBe(1);
    }

    [Fact]
    public async System.Threading.Tasks.Task ReorderProjectTasksAsync_DuplicateId_ThrowsArgumentException()
    {
        // Arrange
        var subscription = CreateSubscription();
        var projectId = Guid.NewGuid();
        var first = new DomainTask(subscription.Id, "First", projectId);

        // Act
        var sut = CreateSut(subscription, new FakeTaskRepository(first), new FakeTaskHistoryRepository());

        // Assert
        await Should.ThrowAsync<ArgumentException>(() => sut.ReorderProjectTasksAsync(projectId, [first.Id, first.Id]));
    }

    [Fact]
    public async System.Threading.Tasks.Task ReorderProjectTasksAsync_UnknownId_ThrowsArgumentException()
    {
        // Arrange
        var subscription = CreateSubscription();
        var projectId = Guid.NewGuid();
        var first = new DomainTask(subscription.Id, "First", projectId);

        // Act
        var sut = CreateSut(subscription, new FakeTaskRepository(first), new FakeTaskHistoryRepository());

        // Assert
        await Should.ThrowAsync<ArgumentException>(() => sut.ReorderProjectTasksAsync(projectId, [Guid.NewGuid()]));
    }

    [Fact]
    public async System.Threading.Tasks.Task UnassignFromProjectAsync_SubTask_ThrowsInvalidOperationException()
    {
        // Arrange
        var subscription = CreateSubscription();
        var parent = new DomainTask(subscription.Id, "Parent", Guid.NewGuid());
        var child = new DomainTask(subscription.Id, "Child", parent.ProjectId);
        parent.AddSubTask(child);

        // Act
        var sut = CreateSut(subscription, new FakeTaskRepository(parent, child), new FakeTaskHistoryRepository());

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(() => sut.UnassignFromProjectAsync(child.Id));
    }

    [Fact]
    public async System.Threading.Tasks.Task UnassignFromProjectAsync_TopLevelTask_ClearsProject()
    {
        // Arrange
        var subscription = CreateSubscription();
        var task = new DomainTask(subscription.Id, "Task", Guid.NewGuid());

        // Act
        var sut = CreateSut(subscription, new FakeTaskRepository(task), new FakeTaskHistoryRepository());

        var updated = await sut.UnassignFromProjectAsync(task.Id);

        // Assert
        updated.ProjectId.ShouldBeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task DueDateAndReminderFlows_PersistChanges()
    {
        // Arrange
        var subscription = CreateSubscription();
        var task = new DomainTask(subscription.Id, "Task", Guid.NewGuid());

        // Act
        var sut = CreateSut(subscription, new FakeTaskRepository(task), new FakeTaskHistoryRepository());

        await sut.SetDueDateAsync(task.Id, new DateOnly(2026, 2, 11));
        await sut.SetDueDateTimeAsync(task.Id, new DateOnly(2026, 2, 11), new TimeOnly(10, 0));

        var withRelativeReminder = await sut.AddRelativeReminderAsync(task.Id, 10);
        var reminderId = withRelativeReminder.Reminders.Single().Id;
        await sut.RemoveReminderAsync(task.Id, reminderId);

        await sut.AddDateOnlyReminderAsync(task.Id, new TimeOnly(8, 30));
        await sut.ClearDueDateAsync(task.Id);
        var toggled = await sut.ToggleTodayMarkAsync(task.Id);

        // Assert
        toggled.IsMarkedForToday.ShouldBeTrue();
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTodayAsync_DueAndMarkedTasks_ReturnsMergedUniqueList()
    {
        // Arrange
        var subscription = CreateSubscription();
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(subscription.TimeZoneId);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));

        var dueToday = new DomainTask(subscription.Id, "Due today", Guid.NewGuid());
        dueToday.SetDueDate(today);

        var markedToday = new DomainTask(subscription.Id, "Marked today", null);
        markedToday.ToggleTodayMark();

        // Act
        var sut = CreateSut(subscription, new FakeTaskRepository(dueToday, markedToday), new FakeTaskHistoryRepository());

        var tasks = await sut.GetTodayAsync();

        // Assert
        tasks.Count.ShouldBe(2);
        tasks.Any(task => task.Id == dueToday.Id).ShouldBeTrue();
        tasks.Any(task => task.Id == markedToday.Id).ShouldBeTrue();
    }

    [Fact]
    public async System.Threading.Tasks.Task RecentWeekUpcomingAndDelete_FlowThroughRepository()
    {
        // Arrange
        var subscription = CreateSubscription();
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(subscription.TimeZoneId);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));
        var projectId = Guid.NewGuid();

        var recent = new DomainTask(subscription.Id, "Recent", projectId);
        var thisWeek = new DomainTask(subscription.Id, "ThisWeek", projectId);
        thisWeek.SetDueDate(today.AddDays(1));
        var upcoming = new DomainTask(subscription.Id, "Upcoming", projectId);
        upcoming.SetDueDate(today.AddDays(14));

        var repository = new FakeTaskRepository(recent, thisWeek, upcoming);

        // Act
        var sut = CreateSut(subscription, repository, new FakeTaskHistoryRepository());

        var recents = await sut.GetRecentAsync();
        var week = await sut.GetThisWeekAsync();
        var up = await sut.GetUpcomingAsync();
        var deleted = await sut.DeleteAsync(recent.Id);

        // Assert
        recents.ShouldNotBeEmpty();
        week.ShouldNotBeNull();
        up.ShouldNotBeEmpty();
        deleted.ShouldBeTrue();
    }

    private static TaskOrchestrator CreateSut(Subscription subscription, FakeTaskRepository repository, FakeTaskHistoryRepository history)
    {
        return new TaskOrchestrator(NullLogger<TaskOrchestrator>.Instance, repository, history, new FakeCurrentSubscriptionAccessor(subscription));
    }

    private static Subscription CreateSubscription()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true, "Europe/Berlin");
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));
        return subscription;
    }

    private sealed class FakeCurrentSubscriptionAccessor : ICurrentSubscriptionAccessor
    {
        private readonly Subscription subscription;

        public FakeCurrentSubscriptionAccessor(Subscription subscription)
        {
            // Arrange
            // Act
            this.subscription = subscription;

            // Assert
        }

        public Subscription GetCurrentSubscription()
        {
            // Arrange
            // Act
            return this.subscription;

            // Assert
        }
    }

    private sealed class FakeTaskRepository : ITaskRepository
    {
        private readonly Dictionary<Guid, DomainTask> store = [];

        public FakeTaskRepository(params DomainTask[] existing)
        {
            // Arrange
            // Act
            foreach (var task in existing)
            {
                this.store[task.Id] = task;
            }

            // Assert
        }

        public int AddCallCount { get; private set; }

        public int UpdateCallCount { get; private set; }

        public Task<List<DomainTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            // Arrange
            // Act
            var result = this.store.Values
                .Where(task => task.ProjectId == projectId && !task.ParentTaskId.HasValue)
                .OrderBy(task => task.SortOrder)
                .ThenBy(task => task.CreatedAt)
                .ToList();
            return System.Threading.Tasks.Task.FromResult(result);

            // Assert
        }

        public Task<List<DomainTask>> GetSubTasksAsync(Guid parentTaskId, CancellationToken cancellationToken = default)
        {
            // Arrange
            // Act
            var result = this.store.Values
                .Where(task => task.ParentTaskId == parentTaskId)
                .OrderBy(task => task.SortOrder)
                .ThenBy(task => task.CreatedAt)
                .ToList();
            return System.Threading.Tasks.Task.FromResult(result);

            // Assert
        }

        public Task<int> GetNextSortOrderAsync(Guid? projectId, Guid? parentTaskId, CancellationToken cancellationToken = default)
        {
            // Arrange
            // Act
            var maxSortOrder = this.store.Values
                .Where(task => task.ProjectId == projectId && task.ParentTaskId == parentTaskId)
                .Select(task => (int?)task.SortOrder)
                .DefaultIfEmpty(null)
                .Max();

            return System.Threading.Tasks.Task.FromResult(maxSortOrder.HasValue ? maxSortOrder.Value + 1 : 0);

            // Assert
        }

        public Task<List<DomainTask>> GetByPriorityAsync(TaskPriority priority, Guid projectId, CancellationToken cancellationToken = default)
        {
            // Arrange
            // Act
            var result = this.store.Values.Where(task => task.ProjectId == projectId && task.Priority == priority).ToList();
            return System.Threading.Tasks.Task.FromResult(result);

            // Assert
        }

        public Task<List<DomainTask>> SearchAsync(string query, Guid projectId, CancellationToken cancellationToken = default)
        {
            // Arrange
            // Act
            var normalized = query.Trim().ToLowerInvariant();
            var result = this.store.Values
                .Where(task => task.ProjectId == projectId)
                .Where(task => task.Title.ToLowerInvariant().Contains(normalized) || (task.Note ?? string.Empty).ToLowerInvariant().Contains(normalized))
                .ToList();
            return System.Threading.Tasks.Task.FromResult(result);

            // Assert
        }

        public Task<List<DomainTask>> GetFocusedAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            // Arrange
            // Act
            var result = this.store.Values.Where(task => task.ProjectId == projectId && task.IsFocused).ToList();
            return System.Threading.Tasks.Task.FromResult(result);

            // Assert
        }

        public Task<List<DomainTask>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            // Arrange
            // Act
            return System.Threading.Tasks.Task.FromResult(this.store.Values.ToList());

            // Assert
        }

        public Task<List<DomainTask>> GetRecentAsync(int days, CancellationToken cancellationToken = default)
        {
            // Arrange
            // Act
            var threshold = DateTime.UtcNow.AddDays(-Math.Abs(days));
            var result = this.store.Values.Where(task => task.CreatedAt >= threshold).OrderByDescending(task => task.CreatedAt).ToList();
            return System.Threading.Tasks.Task.FromResult(result);

            // Assert
        }

        public Task<List<DomainTask>> GetUnassignedRecentAsync(int days, CancellationToken cancellationToken = default)
        {
            // Arrange
            // Act
            var threshold = DateTime.UtcNow.AddDays(-Math.Abs(days));
            var result = this.store.Values.Where(task => !task.ProjectId.HasValue && task.CreatedAt >= threshold).OrderByDescending(task => task.CreatedAt).ToList();
            return System.Threading.Tasks.Task.FromResult(result);

            // Assert
        }

        public Task<List<DomainTask>> GetDueOnDateAsync(DateOnly localDate, CancellationToken cancellationToken = default)
        {
            // Arrange
            // Act
            var result = this.store.Values.Where(task => task.HasDueDate && task.DueDateLocal == localDate).ToList();
            return System.Threading.Tasks.Task.FromResult(result);

            // Assert
        }

        public Task<List<DomainTask>> GetDueInRangeAsync(DateOnly localStartInclusive, DateOnly localEndInclusive, CancellationToken cancellationToken = default)
        {
            // Arrange
            // Act
            var result = this.store.Values.Where(task => task.HasDueDate && task.DueDateLocal >= localStartInclusive && task.DueDateLocal <= localEndInclusive).ToList();
            return System.Threading.Tasks.Task.FromResult(result);

            // Assert
        }

        public Task<List<DomainTask>> GetDueAfterDateAsync(DateOnly localDateExclusive, CancellationToken cancellationToken = default)
        {
            // Arrange
            // Act
            var result = this.store.Values.Where(task => task.HasDueDate && task.DueDateLocal > localDateExclusive).ToList();
            return System.Threading.Tasks.Task.FromResult(result);

            // Assert
        }

        public Task<List<DomainTask>> GetByIdsAsync(IEnumerable<Guid> taskIds, CancellationToken cancellationToken = default)
        {
            // Arrange
            // Act
            var ids = taskIds.ToHashSet();
            var result = this.store.Values.Where(task => ids.Contains(task.Id)).ToList();
            return System.Threading.Tasks.Task.FromResult(result);

            // Assert
        }

        public Task<DomainTask> AddAsync(DomainTask task, CancellationToken cancellationToken = default)
        {
            // Arrange
            // Act
            this.AddCallCount++;
            this.store[task.Id] = task;
            return System.Threading.Tasks.Task.FromResult(task);

            // Assert
        }

        public Task<DomainTask> UpdateAsync(DomainTask task, CancellationToken cancellationToken = default)
        {
            // Arrange
            // Act
            this.UpdateCallCount++;
            this.store[task.Id] = task;
            return System.Threading.Tasks.Task.FromResult(task);

            // Assert
        }

        public Task<List<DomainTask>> UpdateRangeAsync(IEnumerable<DomainTask> tasks, CancellationToken cancellationToken = default)
        {
            // Arrange
            // Act
            var updated = tasks.ToList();
            foreach (var task in updated)
            {
                this.store[task.Id] = task;
            }

            this.UpdateCallCount++;
            return System.Threading.Tasks.Task.FromResult(updated);

            // Assert
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // Arrange
            // Act
            return System.Threading.Tasks.Task.FromResult(this.store.Remove(id));

            // Assert
        }

        public Task<DomainTask> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // Arrange
            // Act
            return System.Threading.Tasks.Task.FromResult(this.store[id]);

            // Assert
        }
    }

    private sealed class FakeTaskHistoryRepository : ITaskHistoryRepository
    {
        public List<(string Name, bool IsSubTaskName)> Registered { get; } = [];

        public List<string> Suggestions { get; set; } = [];

        public string LastPrefix { get; private set; } = string.Empty;

        public bool LastIsSubTaskName { get; private set; }

        public int LastTake { get; private set; }

        public System.Threading.Tasks.Task RegisterUsageAsync(string name, bool isSubTaskName, CancellationToken cancellationToken = default)
        {
            // Arrange
            // Act
            this.Registered.Add((name, isSubTaskName));
            return System.Threading.Tasks.Task.CompletedTask;

            // Assert
        }

        public Task<List<string>> GetSuggestionsAsync(string prefix, bool isSubTaskName, int take = 20, CancellationToken cancellationToken = default)
        {
            // Arrange
            // Act
            this.LastPrefix = prefix;
            this.LastIsSubTaskName = isSubTaskName;
            this.LastTake = take;
            return System.Threading.Tasks.Task.FromResult(this.Suggestions.Take(take).ToList());

            // Assert
        }
    }
}


