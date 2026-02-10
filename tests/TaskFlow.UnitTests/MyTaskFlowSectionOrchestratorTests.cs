using TaskFlow.Application;
using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;

namespace TaskFlow.UnitTests;

/// <summary>
/// Tests My Task Flow section orchestration behavior.
/// </summary>
public class MyTaskFlowSectionOrchestratorTests
{
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
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(subscription.TimeZoneId);
        var todayLocal = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));
        dueTask.SetDueDate(todayLocal);

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

    /// <summary>
    /// Verifies important section returns only starred tasks.
    /// </summary>
    [Fact]
    public async System.Threading.Tasks.Task GetSectionTasksAsync_ImportantSection_ReturnsOnlyImportantTasks()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true, "Europe/Berlin");
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));

        var importantSection = MyTaskFlowSection.CreateSystem(subscription.Id, "Important", 2, TaskFlowDueBucket.Important);

        var starredTask = new DomainTask(subscription.Id, "Starred", Guid.NewGuid());
        starredTask.ToggleImportant();

        var regularTask = new DomainTask(subscription.Id, "Regular", Guid.NewGuid());

        var sectionRepository = new FakeSectionRepository(importantSection);
        var taskRepository = new FakeTaskRepository(starredTask, regularTask);
        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var orchestrator = new MyTaskFlowSectionOrchestrator(sectionRepository, taskRepository, accessor);

        var tasks = await orchestrator.GetSectionTasksAsync(importantSection.Id);

        Assert.Single(tasks);
        Assert.Equal(starredTask.Id, tasks[0].Id);
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
