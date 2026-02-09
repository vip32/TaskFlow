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

        var subscription = new Subscription(DefaultSubscriptionId, "Default", SubscriptionTier.Free, true);
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));

        var inbox = new Project(subscription.Id, "Inbox", "#40E0D0", "inbox", true);
        var personal = new Project(subscription.Id, "Personal", "#10B981", "check_circle");
        var work = new Project(subscription.Id, "Work", "#F57C00", "work");

        var task1 = new DomainTask(subscription.Id, "Review architecture baseline", work.Id);
        task1.SetPriority(TaskPriority.High);
        task1.UpdateNote("Confirm layering and repository boundaries.");

        var task2 = new DomainTask(subscription.Id, "Plan next sprint tasks", work.Id);
        task2.SetPriority(TaskPriority.Medium);
        task2.ToggleFocus();

        var task3 = new DomainTask(subscription.Id, "Organize personal errands", personal.Id);
        task3.SetPriority(TaskPriority.Low);

        var task4 = new DomainTask(subscription.Id, "Capture inbox ideas", inbox.Id);
        task4.UpdateNote("Use this task as a sample for first-run onboarding.");

        db.Subscriptions.Add(subscription);
        db.Projects.AddRange(inbox, personal, work);
        db.Tasks.AddRange(task1, task2, task3, task4);

        db.SaveChanges();
    }

    /// <summary>
    /// Seeds initial data asynchronously into the database when no subscription exists.
    /// </summary>
    /// <param name="db">Database context instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing async seeding completion.</returns>
    public static async global::System.Threading.Tasks.Task SeedAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        if (await db.Subscriptions.AnyAsync(cancellationToken))
        {
            return;
        }

        var subscription = new Subscription(DefaultSubscriptionId, "Default", SubscriptionTier.Free, true);
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));

        var inbox = new Project(subscription.Id, "Inbox", "#40E0D0", "inbox", true);
        var personal = new Project(subscription.Id, "Personal", "#10B981", "check_circle");
        var work = new Project(subscription.Id, "Work", "#F57C00", "work");

        var task1 = new DomainTask(subscription.Id, "Review architecture baseline", work.Id);
        task1.SetPriority(TaskPriority.High);
        task1.UpdateNote("Confirm layering and repository boundaries.");

        var task2 = new DomainTask(subscription.Id, "Plan next sprint tasks", work.Id);
        task2.SetPriority(TaskPriority.Medium);
        task2.ToggleFocus();

        var task3 = new DomainTask(subscription.Id, "Organize personal errands", personal.Id);
        task3.SetPriority(TaskPriority.Low);

        var task4 = new DomainTask(subscription.Id, "Capture inbox ideas", inbox.Id);
        task4.UpdateNote("Use this task as a sample for first-run onboarding.");

        db.Subscriptions.Add(subscription);
        db.Projects.AddRange(inbox, personal, work);
        db.Tasks.AddRange(task1, task2, task3, task4);

        await db.SaveChangesAsync(cancellationToken);
    }
}
