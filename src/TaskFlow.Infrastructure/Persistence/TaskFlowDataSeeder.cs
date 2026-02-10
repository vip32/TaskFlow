using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;

namespace TaskFlow.Infrastructure.Persistence;

/// <summary>
/// Seeds baseline TaskFlow data for local development and first-time startup.
/// </summary>
public static class TaskFlowDataSeeder
{
    private static readonly Guid DefaultSubscriptionId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    /// <summary>
    /// Seeds initial data into the database when no subscription exists.
    /// </summary>
    /// <param name="db">Database context instance.</param>
    public static void Seed(AppDbContext db)
    {
        if (db.Subscriptions.Any())
        {
            return;
        }

        var subscription = new Subscription(DefaultSubscriptionId, "Default", SubscriptionTier.Free, true, "Europe/Berlin");
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));

        var backlog = new Project(subscription.Id, "Backlog", "#40E0D0", "inbox", note: "General engineering backlog.", isDefault: true);
        backlog.SetTags(["Study"]);

        var apiProject = new Project(subscription.Id, "API Service", "#10B981", "dns", note: "Backend and API implementation work.");
        apiProject.SetTags(["Bug", "Study"]);

        var webProject = new Project(subscription.Id, "Web UI", "#F57C00", "web", note: "Blazor UI and interaction work.");
        webProject.SetTags(["Read", "Study"]);

        var task1 = new DomainTask(subscription.Id, "Fix authentication token refresh bug", apiProject.Id);
        task1.SetPriority(TaskPriority.High);
        task1.UpdateNote("Investigate refresh pipeline and patch retry behavior.");
        task1.SetTags(["Bug"]);

        var task2 = new DomainTask(subscription.Id, "Read .NET 10 EF Core seeding docs", backlog.Id);
        task2.SetPriority(TaskPriority.Medium);
        task2.UpdateNote("Review new seeding APIs and migration implications.");
        task2.SetTags(["Read"]);

        var task3 = new DomainTask(subscription.Id, "Study Injectio attribute registration patterns", webProject.Id);
        task3.SetPriority(TaskPriority.Medium);
        task3.ToggleFocus();
        task3.SetTags(["Study"]);

        var task4 = new DomainTask(subscription.Id, "Refine task board drag-and-drop UX", webProject.Id);
        task4.SetPriority(TaskPriority.Low);
        task4.SetTags(["Read", "Study"]);

        var task5 = new DomainTask(subscription.Id, "Triage incoming feature requests", null);
        task5.SetPriority(TaskPriority.Medium);
        task5.SetTags(["Study"]);

        db.Subscriptions.Add(subscription);
        db.Projects.AddRange(backlog, apiProject, webProject);
        db.Tasks.AddRange(task1, task2, task3, task4, task5);

        db.MyTaskFlowSections.AddRange(
            MyTaskFlowSection.CreateSystem(subscription.Id, "Recent", 0, TaskFlowDueBucket.Recent),
            MyTaskFlowSection.CreateSystem(subscription.Id, "Today", 1, TaskFlowDueBucket.Today),
            MyTaskFlowSection.CreateSystem(subscription.Id, "Important", 2, TaskFlowDueBucket.Important),
            MyTaskFlowSection.CreateSystem(subscription.Id, "This Week", 3, TaskFlowDueBucket.ThisWeek),
            MyTaskFlowSection.CreateSystem(subscription.Id, "Upcoming", 4, TaskFlowDueBucket.Upcoming));

        db.SaveChanges();
    }

    /// <summary>
    /// Seeds initial data asynchronously into the database when no subscription exists.
    /// </summary>
    /// <param name="db">Database context instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing async seeding completion.</returns>
    public static async System.Threading.Tasks.Task SeedAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        if (await db.Subscriptions.AnyAsync(cancellationToken))
        {
            return;
        }

        var subscription = new Subscription(DefaultSubscriptionId, "Default", SubscriptionTier.Free, true, "Europe/Berlin");
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));

        var backlog = new Project(subscription.Id, "Backlog", "#40E0D0", "inbox", note: "General engineering backlog.", isDefault: true);
        backlog.SetTags(["Study"]);

        var apiProject = new Project(subscription.Id, "API Service", "#10B981", "dns", note: "Backend and API implementation work.");
        apiProject.SetTags(["Bug", "Study"]);

        var webProject = new Project(subscription.Id, "Web UI", "#F57C00", "web", note: "Blazor UI and interaction work.");
        webProject.SetTags(["Read", "Study"]);

        var task1 = new DomainTask(subscription.Id, "Fix authentication token refresh bug", apiProject.Id);
        task1.SetPriority(TaskPriority.High);
        task1.UpdateNote("Investigate refresh pipeline and patch retry behavior.");
        task1.SetTags(["Bug"]);

        var task2 = new DomainTask(subscription.Id, "Read .NET 10 EF Core seeding docs", backlog.Id);
        task2.SetPriority(TaskPriority.Medium);
        task2.UpdateNote("Review new seeding APIs and migration implications.");
        task2.SetTags(["Read"]);

        var task3 = new DomainTask(subscription.Id, "Study Injectio attribute registration patterns", webProject.Id);
        task3.SetPriority(TaskPriority.Medium);
        task3.ToggleFocus();
        task3.SetTags(["Study"]);

        var task4 = new DomainTask(subscription.Id, "Refine task board drag-and-drop UX", webProject.Id);
        task4.SetPriority(TaskPriority.Low);
        task4.SetTags(["Read", "Study"]);

        var task5 = new DomainTask(subscription.Id, "Triage incoming feature requests", null);
        task5.SetPriority(TaskPriority.Medium);
        task5.SetTags(["Study"]);

        db.Subscriptions.Add(subscription);
        db.Projects.AddRange(backlog, apiProject, webProject);
        db.Tasks.AddRange(task1, task2, task3, task4, task5);

        db.MyTaskFlowSections.AddRange(
            MyTaskFlowSection.CreateSystem(subscription.Id, "Recent", 0, TaskFlowDueBucket.Recent),
            MyTaskFlowSection.CreateSystem(subscription.Id, "Today", 1, TaskFlowDueBucket.Today),
            MyTaskFlowSection.CreateSystem(subscription.Id, "Important", 2, TaskFlowDueBucket.Important),
            MyTaskFlowSection.CreateSystem(subscription.Id, "This Week", 3, TaskFlowDueBucket.ThisWeek),
            MyTaskFlowSection.CreateSystem(subscription.Id, "Upcoming", 4, TaskFlowDueBucket.Upcoming));

        await db.SaveChangesAsync(cancellationToken);
    }
}
