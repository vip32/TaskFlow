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
    public async global::System.Threading.Tasks.Task GetSectionTasksAsync_RuleAndManualMembership_ReturnsExpectedTasks()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true, "Europe/Berlin");
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));

        var todaySection = MyTaskFlowSection.CreateSystem(subscription.Id, "Today", 1, TaskFlowDueBucket.Today);
        var dueTask = new DomainTask(subscription.Id, "Due", Guid.NewGuid());
        dueTask.SetDueDate(DateOnly.FromDateTime(DateTime.UtcNow));

        var manualTask = new DomainTask(subscription.Id, "Manual", Guid.Empty);
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

        public global::System.Threading.Tasks.Task<List<MyTaskFlowSection>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.sections.Values.ToList());
        }

        public global::System.Threading.Tasks.Task<MyTaskFlowSection> GetByIdAsync(Guid sectionId, CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.sections[sectionId]);
        }

        public global::System.Threading.Tasks.Task<MyTaskFlowSection> AddAsync(MyTaskFlowSection section, CancellationToken cancellationToken = default)
        {
            this.sections[section.Id] = section;
            return global::System.Threading.Tasks.Task.FromResult(section);
        }

        public global::System.Threading.Tasks.Task<MyTaskFlowSection> UpdateAsync(MyTaskFlowSection section, CancellationToken cancellationToken = default)
        {
            this.sections[section.Id] = section;
            return global::System.Threading.Tasks.Task.FromResult(section);
        }

        public global::System.Threading.Tasks.Task<bool> DeleteAsync(Guid sectionId, CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.sections.Remove(sectionId));
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

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.tasks.Values.Where(task => task.ProjectId == projectId).ToList());
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetSubTasksAsync(Guid parentTaskId, CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.tasks.Values.Where(task => task.ParentTaskId == parentTaskId).ToList());
        }

        public global::System.Threading.Tasks.Task<int> GetNextSortOrderAsync(Guid projectId, Guid parentTaskId, CancellationToken cancellationToken = default)
        {
            var maxSortOrder = this.tasks.Values
                .Where(task => task.ProjectId == projectId && task.ParentTaskId == parentTaskId)
                .Select(task => (int?)task.SortOrder)
                .DefaultIfEmpty(null)
                .Max();

            return global::System.Threading.Tasks.Task.FromResult(maxSortOrder.HasValue ? maxSortOrder.Value + 1 : 0);
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetByPriorityAsync(TaskPriority priority, Guid projectId, CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.tasks.Values.Where(task => task.ProjectId == projectId && task.Priority == priority).ToList());
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> SearchAsync(string query, Guid projectId, CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.tasks.Values.Where(task => task.ProjectId == projectId).ToList());
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetFocusedAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.tasks.Values.Where(task => task.ProjectId == projectId && task.IsFocused).ToList());
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.tasks.Values.ToList());
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetRecentAsync(int days, CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.tasks.Values.ToList());
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetUnassignedRecentAsync(int days, CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.tasks.Values.Where(task => task.ProjectId == Guid.Empty).ToList());
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetDueOnDateAsync(DateOnly localDate, CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.tasks.Values.Where(task => task.HasDueDate && task.DueDateLocal == localDate).ToList());
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetDueInRangeAsync(DateOnly localStartInclusive, DateOnly localEndInclusive, CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.tasks.Values.Where(task => task.HasDueDate && task.DueDateLocal >= localStartInclusive && task.DueDateLocal <= localEndInclusive).ToList());
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetDueAfterDateAsync(DateOnly localDateExclusive, CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.tasks.Values.Where(task => task.HasDueDate && task.DueDateLocal > localDateExclusive).ToList());
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetByIdsAsync(IEnumerable<Guid> taskIds, CancellationToken cancellationToken = default)
        {
            var ids = taskIds.ToHashSet();
            return global::System.Threading.Tasks.Task.FromResult(this.tasks.Values.Where(task => ids.Contains(task.Id)).ToList());
        }

        public global::System.Threading.Tasks.Task<DomainTask> AddAsync(DomainTask task, CancellationToken cancellationToken = default)
        {
            this.tasks[task.Id] = task;
            return global::System.Threading.Tasks.Task.FromResult(task);
        }

        public global::System.Threading.Tasks.Task<DomainTask> UpdateAsync(DomainTask task, CancellationToken cancellationToken = default)
        {
            this.tasks[task.Id] = task;
            return global::System.Threading.Tasks.Task.FromResult(task);
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> UpdateRangeAsync(IEnumerable<DomainTask> tasks, CancellationToken cancellationToken = default)
        {
            var updated = tasks.ToList();
            foreach (var task in updated)
            {
                this.tasks[task.Id] = task;
            }

            return global::System.Threading.Tasks.Task.FromResult(updated);
        }

        public global::System.Threading.Tasks.Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.tasks.Remove(id));
        }

        public global::System.Threading.Tasks.Task<DomainTask> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.tasks[id]);
        }
    }
}
