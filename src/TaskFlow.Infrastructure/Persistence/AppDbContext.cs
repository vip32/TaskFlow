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
            entity.Property(p => p.CreatedAt).IsRequired();
            entity.Property(p => p.IsDefault).IsRequired();
            entity.Property(p => p.ViewType).HasConversion<int>().IsRequired();
            entity.Ignore(p => p.Tasks);

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
            entity.Ignore(t => t.SubTasks);

            entity.HasOne<TaskFlow.Domain.Project>()
                .WithMany()
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<TaskFlow.Domain.Subscription>()
                .WithMany()
                .HasForeignKey(t => t.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(t => new { t.SubscriptionId, t.ProjectId, t.CreatedAt });
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

        base.OnModelCreating(modelBuilder);
    }
}
