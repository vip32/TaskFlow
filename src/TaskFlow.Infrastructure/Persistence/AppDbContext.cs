using Microsoft.EntityFrameworkCore;
using DomainTask = TaskFlow.Domain.Task;

namespace TaskFlow.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for TaskFlow persistence.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    /// <param name="options">Configured DbContext options.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the subscriptions set.
    /// </summary>
    public DbSet<TaskFlow.Domain.Subscription> Subscriptions => this.Set<TaskFlow.Domain.Subscription>();

    /// <summary>
    /// Gets the subscription schedules set.
    /// </summary>
    public DbSet<TaskFlow.Domain.SubscriptionSchedule> SubscriptionSchedules => this.Set<TaskFlow.Domain.SubscriptionSchedule>();

    /// <summary>
    /// Gets the task history set.
    /// </summary>
    public DbSet<TaskFlow.Domain.TaskHistory> TaskHistories => this.Set<TaskFlow.Domain.TaskHistory>();

    /// <summary>
    /// Gets the projects set.
    /// </summary>
    public DbSet<TaskFlow.Domain.Project> Projects => this.Set<TaskFlow.Domain.Project>();

    /// <summary>
    /// Gets the tasks set.
    /// </summary>
    public DbSet<DomainTask> Tasks => this.Set<DomainTask>();

    /// <summary>
    /// Gets the focus sessions set.
    /// </summary>
    public DbSet<TaskFlow.Domain.FocusSession> FocusSessions => this.Set<TaskFlow.Domain.FocusSession>();

    /// <summary>
    /// Gets task reminders set.
    /// </summary>
    public DbSet<TaskFlow.Domain.TaskReminder> TaskReminders => this.Set<TaskFlow.Domain.TaskReminder>();

    /// <summary>
    /// Gets My Task Flow sections set.
    /// </summary>
    public DbSet<TaskFlow.Domain.MyTaskFlowSection> MyTaskFlowSections => this.Set<TaskFlow.Domain.MyTaskFlowSection>();

    /// <summary>
    /// Gets My Task Flow section task links set.
    /// </summary>
    public DbSet<TaskFlow.Domain.MyTaskFlowSectionTask> MyTaskFlowSectionTasks => this.Set<TaskFlow.Domain.MyTaskFlowSectionTask>();

    /// <summary>
    /// Configures entity mappings for the TaskFlow domain model.
    /// </summary>
    /// <param name="modelBuilder">Model builder instance.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskFlow.Domain.Subscription>(entity =>
        {
            entity.ToTable("Subscriptions");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
            entity.Property(s => s.Tier).HasConversion<int>().IsRequired();
            entity.Property(s => s.IsEnabled).IsRequired();
            entity.Property(s => s.CreatedAt).IsRequired();
            entity.Property(s => s.TimeZoneId).IsRequired().HasMaxLength(128);
            entity.Ignore(s => s.Schedules);
        });

        modelBuilder.Entity<TaskFlow.Domain.SubscriptionSchedule>(entity =>
        {
            entity.ToTable("SubscriptionSchedules");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.SubscriptionId).IsRequired();
            entity.Property(s => s.StartsOn).IsRequired();
            entity.Property(s => s.EndsOn).IsRequired();
            entity.Ignore(s => s.IsOpenEnded);

            entity.HasOne<TaskFlow.Domain.Subscription>()
                .WithMany()
                .HasForeignKey(s => s.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(s => new { s.SubscriptionId, s.StartsOn });
        });

        modelBuilder.Entity<TaskFlow.Domain.Project>(entity =>
        {
            entity.ToTable("Projects");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.SubscriptionId).IsRequired();
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Color).IsRequired().HasMaxLength(32);
            entity.Property(p => p.Icon).IsRequired().HasMaxLength(64);
            entity.Property(p => p.Note).IsRequired().HasMaxLength(4000);
            entity.Property(p => p.CreatedAt).IsRequired();
            entity.Property(p => p.IsDefault).IsRequired();
            entity.Property(p => p.ViewType).HasConversion<int>().IsRequired();
            entity.Ignore(p => p.Tasks);
            entity.Ignore(p => p.Tags);

            entity.HasOne<TaskFlow.Domain.Subscription>()
                .WithMany()
                .HasForeignKey(p => p.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(p => new { p.SubscriptionId, p.Name });
        });

        modelBuilder.Entity<DomainTask>(entity =>
        {
            entity.ToTable("Tasks");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.SubscriptionId).IsRequired();
            entity.Property(t => t.Title).IsRequired().HasMaxLength(300);
            entity.Property(t => t.Note).IsRequired().HasMaxLength(4000);
            entity.Property(t => t.Priority).HasConversion<int>().IsRequired();
            entity.Property(t => t.Status).HasConversion<int>().IsRequired();
            entity.Property(t => t.IsCompleted).IsRequired();
            entity.Property(t => t.IsFocused).IsRequired();
            entity.Property(t => t.ProjectId).IsRequired();
            entity.Property(t => t.ParentTaskId).IsRequired();
            entity.Property(t => t.CreatedAt).IsRequired();
            entity.Property(t => t.CompletedAt).IsRequired();
            entity.Property(t => t.HasDueDate).IsRequired();
            entity.Property(t => t.DueDateLocal).IsRequired();
            entity.Property(t => t.HasDueTime).IsRequired();
            entity.Property(t => t.DueTimeLocal).IsRequired();
            entity.Property(t => t.DueAtUtc).IsRequired();
            entity.Property(t => t.IsMarkedForToday).IsRequired();
            entity.Ignore(t => t.SubTasks);
            entity.Ignore(t => t.Tags);
            entity.HasMany(t => t.Reminders)
                .WithOne()
                .HasForeignKey(r => r.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<TaskFlow.Domain.Project>()
                .WithMany()
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            entity.HasOne<TaskFlow.Domain.Subscription>()
                .WithMany()
                .HasForeignKey(t => t.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(t => new { t.SubscriptionId, t.ProjectId, t.CreatedAt });
            entity.HasIndex(t => new { t.SubscriptionId, t.DueDateLocal });
            entity.HasIndex(t => new { t.SubscriptionId, t.DueAtUtc });
        });

        modelBuilder.Entity<TaskFlow.Domain.FocusSession>(entity =>
        {
            entity.ToTable("FocusSessions");
            entity.HasKey(f => f.Id);
            entity.Property(f => f.SubscriptionId).IsRequired();
            entity.Property(f => f.TaskId).IsRequired();
            entity.Property(f => f.StartedAt).IsRequired();
            entity.Property(f => f.EndedAt).IsRequired();
            entity.Ignore(f => f.IsCompleted);
            entity.Ignore(f => f.Duration);

            entity.HasOne<TaskFlow.Domain.Subscription>()
                .WithMany()
                .HasForeignKey(f => f.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(f => new { f.SubscriptionId, f.TaskId, f.StartedAt });
        });

        modelBuilder.Entity<TaskFlow.Domain.TaskHistory>(entity =>
        {
            entity.ToTable("TaskHistory");
            entity.HasKey(h => h.Id);
            entity.Property(h => h.SubscriptionId).IsRequired();
            entity.Property(h => h.Name).IsRequired().HasMaxLength(500);
            entity.Property(h => h.IsSubTaskName).IsRequired();
            entity.Property(h => h.LastUsedAt).IsRequired();
            entity.Property(h => h.UsageCount).IsRequired();

            entity.HasOne<TaskFlow.Domain.Subscription>()
                .WithMany()
                .HasForeignKey(h => h.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(h => new { h.SubscriptionId, h.IsSubTaskName, h.Name });
            entity.HasIndex(h => new { h.SubscriptionId, h.LastUsedAt });
        });

        modelBuilder.Entity<TaskFlow.Domain.TaskReminder>(entity =>
        {
            entity.ToTable("TaskReminders");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.TaskId).IsRequired();
            entity.Property(r => r.Mode).HasConversion<int>().IsRequired();
            entity.Property(r => r.MinutesBefore).IsRequired();
            entity.Property(r => r.FallbackLocalTime).IsRequired();
            entity.Property(r => r.TriggerAtUtc).IsRequired();
            entity.Property(r => r.SentAtUtc).IsRequired();

            entity.HasIndex(r => new { r.TaskId, r.TriggerAtUtc, r.SentAtUtc });
        });

        modelBuilder.Entity<TaskFlow.Domain.MyTaskFlowSection>(entity =>
        {
            entity.ToTable("MyTaskFlowSections");
            entity.HasKey(section => section.Id);
            entity.Property(section => section.SubscriptionId).IsRequired();
            entity.Property(section => section.Name).IsRequired().HasMaxLength(200);
            entity.Property(section => section.SortOrder).IsRequired();
            entity.Property(section => section.IsSystemSection).IsRequired();
            entity.Property(section => section.DueBucket).HasConversion<int>().IsRequired();
            entity.Property(section => section.IncludeAssignedTasks).IsRequired();
            entity.Property(section => section.IncludeUnassignedTasks).IsRequired();
            entity.Property(section => section.IncludeDoneTasks).IsRequired();
            entity.Property(section => section.IncludeCancelledTasks).IsRequired();

            entity.HasOne<TaskFlow.Domain.Subscription>()
                .WithMany()
                .HasForeignKey(section => section.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(section => section.ManualTasks)
                .WithOne()
                .HasForeignKey(link => link.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(section => new { section.SubscriptionId, section.SortOrder });
        });

        modelBuilder.Entity<TaskFlow.Domain.MyTaskFlowSectionTask>(entity =>
        {
            entity.ToTable("MyTaskFlowSectionTasks");
            entity.HasKey(link => new { link.SectionId, link.TaskId });
            entity.Property(link => link.SectionId).IsRequired();
            entity.Property(link => link.TaskId).IsRequired();

            entity.HasOne<DomainTask>()
                .WithMany()
                .HasForeignKey(link => link.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}
