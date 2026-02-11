using TaskFlow.Application;
using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;

namespace TaskFlow.UnitTests.Application;

/// <summary>
/// Tests My Task Flow section orchestration behavior.
/// </summary>
public class MyTaskFlowSectionOrchestratorTests
{
    [Fact]
    public async System.Threading.Tasks.Task GetAllAsync_ReturnsSections()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true, "Europe/Berlin");
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));
        var sectionA = new MyTaskFlowSection(subscription.Id, "A", 0);
        var sectionB = new MyTaskFlowSection(subscription.Id, "B", 1);
        var sectionRepository = new FakeSectionRepository(sectionA, sectionB);
        var taskRepository = new FakeTaskRepository();
        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var orchestrator = new MyTaskFlowSectionOrchestrator(sectionRepository, taskRepository, accessor);

        var sections = await orchestrator.GetAllAsync();

        Assert.Equal(2, sections.Count);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_ValidInput_UsesCurrentSubscription()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true, "Europe/Berlin");
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));
        var sectionRepository = new FakeSectionRepository();
        var taskRepository = new FakeTaskRepository();
        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var orchestrator = new MyTaskFlowSectionOrchestrator(sectionRepository, taskRepository, accessor);

        var created = await orchestrator.CreateAsync("Inbox", 5);

        Assert.Equal(subscription.Id, created.SubscriptionId);
        Assert.Equal("Inbox", created.Name);
        Assert.Equal(5, created.SortOrder);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateRuleAsync_ExistingSection_PersistsRule()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true, "Europe/Berlin");
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));
        var section = new MyTaskFlowSection(subscription.Id, "Inbox", 1);
        var sectionRepository = new FakeSectionRepository(section);
        var taskRepository = new FakeTaskRepository();
        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var orchestrator = new MyTaskFlowSectionOrchestrator(sectionRepository, taskRepository, accessor);

        var updated = await orchestrator.UpdateRuleAsync(section.Id, TaskFlowDueBucket.NoDueDate, false, true, true, true);

        Assert.Equal(TaskFlowDueBucket.NoDueDate, updated.DueBucket);
        Assert.False(updated.IncludeAssignedTasks);
        Assert.True(updated.IncludeUnassignedTasks);
        Assert.True(updated.IncludeDoneTasks);
        Assert.True(updated.IncludeCancelledTasks);
    }

    [Fact]
    public async System.Threading.Tasks.Task IncludeTaskAsync_ExistingSection_AddsManualTask()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true, "Europe/Berlin");
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));
        var section = new MyTaskFlowSection(subscription.Id, "Inbox", 1);
        var sectionRepository = new FakeSectionRepository(section);
        var taskRepository = new FakeTaskRepository();
        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var orchestrator = new MyTaskFlowSectionOrchestrator(sectionRepository, taskRepository, accessor);
        var taskId = Guid.NewGuid();

        var updated = await orchestrator.IncludeTaskAsync(section.Id, taskId);

        Assert.Contains(updated.ManualTasks, entry => entry.TaskId == taskId);
    }

    [Fact]
    public async System.Threading.Tasks.Task RemoveTaskAsync_ManualTaskIncluded_RemovesTask()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true, "Europe/Berlin");
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));
        var section = new MyTaskFlowSection(subscription.Id, "Inbox", 1);
        var taskId = Guid.NewGuid();
        section.IncludeTask(taskId);
        var sectionRepository = new FakeSectionRepository(section);
        var taskRepository = new FakeTaskRepository();
        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var orchestrator = new MyTaskFlowSectionOrchestrator(sectionRepository, taskRepository, accessor);

        var updated = await orchestrator.RemoveTaskAsync(section.Id, taskId);

        Assert.DoesNotContain(updated.ManualTasks, entry => entry.TaskId == taskId);
    }

    /// <summary>
    /// Verifies section tasks are resolved using rules and manual curation.
    /// </summary>
    [Fact]
    public async System.Threading.Tasks.Task GetSectionTasksAsync_RuleAndManualMembership_ReturnsExpectedTasks()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true, "Europe/Berlin");
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));

        var todaySection = MyTaskFlowSection.CreateSystem(subscription.Id, "Today", 1, TaskFlowDueBucket.Today);
        var dueTask = new DomainTask(subscription.Id, "Due", Guid.NewGuid());
        dueTask.SetDueDate(DateOnly.FromDateTime(DateTime.UtcNow));

        var manualTask = new DomainTask(subscription.Id, "Manual", null);
        todaySection.IncludeTask(manualTask.Id);

        var sectionRepository = new FakeSectionRepository(todaySection);
        var taskRepository = new FakeTaskRepository(dueTask, manualTask);
        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var orchestrator = new MyTaskFlowSectionOrchestrator(sectionRepository, taskRepository, accessor);

        var tasks = await orchestrator.GetSectionTasksAsync(todaySection.Id);

        Assert.Equal(2, tasks.Count);
        Assert.Contains(tasks, task => task.Id == dueTask.Id);
        Assert.Contains(tasks, task => task.Id == manualTask.Id);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetSectionTasksAsync_MultipleMatches_ReturnsSortedByDueAndCreated()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true, "Europe/Berlin");
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));

        var section = MyTaskFlowSection.CreateSystem(subscription.Id, "Today", 1, TaskFlowDueBucket.Any);
        var noDueOlder = new DomainTask(subscription.Id, "No due older", null);
        await System.Threading.Tasks.Task.Delay(10);
        var noDueNewer = new DomainTask(subscription.Id, "No due newer", null);
        var withDue = new DomainTask(subscription.Id, "With due", null);
        withDue.SetDueDate(DateOnly.FromDateTime(DateTime.UtcNow));

        var sectionRepository = new FakeSectionRepository(section);
        var taskRepository = new FakeTaskRepository(noDueNewer, noDueOlder, withDue);
        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var orchestrator = new MyTaskFlowSectionOrchestrator(sectionRepository, taskRepository, accessor);

        var tasks = await orchestrator.GetSectionTasksAsync(section.Id);

        Assert.Equal(withDue.Id, tasks[0].Id);
        Assert.Equal(noDueNewer.Id, tasks[1].Id);
        Assert.Equal(noDueOlder.Id, tasks[2].Id);
    }

    private sealed class FakeCurrentSubscriptionAccessor : ICurrentSubscriptionAccessor
    {
        private readonly Subscription subscription;

        public FakeCurrentSubscriptionAccessor(Subscription subscription)
        {
            this.subscription = subscription;
        }

        public Subscription GetCurrentSubscription()
        {
            return this.subscription;
        }
    }

    private sealed class FakeSectionRepository : IMyTaskFlowSectionRepository
    {
        private readonly Dictionary<Guid, MyTaskFlowSection> sections = [];

        public FakeSectionRepository(params MyTaskFlowSection[] initial)
        {
            foreach (var section in initial)
            {
                this.sections[section.Id] = section;
            }
        }

        public Task<List<MyTaskFlowSection>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.sections.Values.ToList());
        }

        public Task<MyTaskFlowSection> GetByIdAsync(Guid sectionId, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.sections[sectionId]);
        }

        public Task<MyTaskFlowSection> AddAsync(MyTaskFlowSection section, CancellationToken cancellationToken = default)
        {
            this.sections[section.Id] = section;
            return System.Threading.Tasks.Task.FromResult(section);
        }

        public Task<MyTaskFlowSection> UpdateAsync(MyTaskFlowSection section, CancellationToken cancellationToken = default)
        {
            this.sections[section.Id] = section;
            return System.Threading.Tasks.Task.FromResult(section);
        }

        public Task<bool> DeleteAsync(Guid sectionId, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.sections.Remove(sectionId));
        }
    }

    private sealed class FakeTaskRepository : ITaskRepository
    {
        private readonly Dictionary<Guid, DomainTask> tasks = [];

        public FakeTaskRepository(params DomainTask[] initial)
        {
            foreach (var task in initial)
            {
                this.tasks[task.Id] = task;
            }
        }

        public Task<List<DomainTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.tasks.Values.Where(task => task.ProjectId == projectId).ToList());
        }

        public Task<List<DomainTask>> GetSubTasksAsync(Guid parentTaskId, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.tasks.Values.Where(task => task.ParentTaskId == parentTaskId).ToList());
        }

        public Task<int> GetNextSortOrderAsync(Guid? projectId, Guid? parentTaskId, CancellationToken cancellationToken = default)
        {
            var maxSortOrder = this.tasks.Values
                .Where(task => task.ProjectId == projectId && task.ParentTaskId == parentTaskId)
                .Select(task => (int?)task.SortOrder)
                .DefaultIfEmpty(null)
                .Max();

            return System.Threading.Tasks.Task.FromResult(maxSortOrder.HasValue ? maxSortOrder.Value + 1 : 0);
        }

        public Task<List<DomainTask>> GetByPriorityAsync(TaskPriority priority, Guid projectId, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.tasks.Values.Where(task => task.ProjectId == projectId && task.Priority == priority).ToList());
        }

        public Task<List<DomainTask>> SearchAsync(string query, Guid projectId, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.tasks.Values.Where(task => task.ProjectId == projectId).ToList());
        }

        public Task<List<DomainTask>> GetFocusedAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.tasks.Values.Where(task => task.ProjectId == projectId && task.IsFocused).ToList());
        }

        public Task<List<DomainTask>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.tasks.Values.ToList());
        }

        public Task<List<DomainTask>> GetRecentAsync(int days, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.tasks.Values.ToList());
        }

        public Task<List<DomainTask>> GetUnassignedRecentAsync(int days, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.tasks.Values.Where(task => !task.ProjectId.HasValue).ToList());
        }

        public Task<List<DomainTask>> GetDueOnDateAsync(DateOnly localDate, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.tasks.Values.Where(task => task.HasDueDate && task.DueDateLocal == localDate).ToList());
        }

        public Task<List<DomainTask>> GetDueInRangeAsync(DateOnly localStartInclusive, DateOnly localEndInclusive, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.tasks.Values.Where(task => task.HasDueDate && task.DueDateLocal >= localStartInclusive && task.DueDateLocal <= localEndInclusive).ToList());
        }

        public Task<List<DomainTask>> GetDueAfterDateAsync(DateOnly localDateExclusive, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.tasks.Values.Where(task => task.HasDueDate && task.DueDateLocal > localDateExclusive).ToList());
        }

        public Task<List<DomainTask>> GetByIdsAsync(IEnumerable<Guid> taskIds, CancellationToken cancellationToken = default)
        {
            var ids = taskIds.ToHashSet();
            return System.Threading.Tasks.Task.FromResult(this.tasks.Values.Where(task => ids.Contains(task.Id)).ToList());
        }

        public Task<DomainTask> AddAsync(DomainTask task, CancellationToken cancellationToken = default)
        {
            this.tasks[task.Id] = task;
            return System.Threading.Tasks.Task.FromResult(task);
        }

        public Task<DomainTask> UpdateAsync(DomainTask task, CancellationToken cancellationToken = default)
        {
            this.tasks[task.Id] = task;
            return System.Threading.Tasks.Task.FromResult(task);
        }

        public Task<List<DomainTask>> UpdateRangeAsync(IEnumerable<DomainTask> tasks, CancellationToken cancellationToken = default)
        {
            var updated = tasks.ToList();
            foreach (var task in updated)
            {
                this.tasks[task.Id] = task;
            }

            return System.Threading.Tasks.Task.FromResult(updated);
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.tasks.Remove(id));
        }

        public Task<DomainTask> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.tasks[id]);
        }
    }
}

