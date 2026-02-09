using Injectio.Attributes;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for My Task Flow section persistence.
/// </summary>
[RegisterScoped(ServiceType = typeof(IMyTaskFlowSectionRepository))]
public sealed class MyTaskFlowSectionRepository : IMyTaskFlowSectionRepository
{
    private readonly IDbContextFactory<AppDbContext> factory;
    private readonly ICurrentSubscriptionAccessor currentSubscriptionAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyTaskFlowSectionRepository"/> class.
    /// </summary>
    /// <param name="factory">DbContext factory.</param>
    /// <param name="currentSubscriptionAccessor">Current subscription accessor.</param>
    public MyTaskFlowSectionRepository(IDbContextFactory<AppDbContext> factory, ICurrentSubscriptionAccessor currentSubscriptionAccessor)
    {
        this.factory = factory;
        this.currentSubscriptionAccessor = currentSubscriptionAccessor;
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<List<MyTaskFlowSection>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        return await db.MyTaskFlowSections
            .AsNoTracking()
            .Where(section => section.SubscriptionId == subscriptionId)
            .OrderBy(section => section.SortOrder)
            .ThenBy(section => section.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<MyTaskFlowSection> GetByIdAsync(Guid sectionId, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        var section = await db.MyTaskFlowSections
            .FirstOrDefaultAsync(candidate => candidate.SubscriptionId == subscriptionId && candidate.Id == sectionId, cancellationToken);

        if (section is null)
        {
            throw new KeyNotFoundException($"Section with id '{sectionId}' was not found.");
        }

        return section;
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<MyTaskFlowSection> AddAsync(MyTaskFlowSection section, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(section);
        EnsureSubscriptionMatch(section.SubscriptionId);

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        db.MyTaskFlowSections.Add(section);
        await db.SaveChangesAsync(cancellationToken);
        return section;
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<MyTaskFlowSection> UpdateAsync(MyTaskFlowSection section, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(section);
        EnsureSubscriptionMatch(section.SubscriptionId);

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        db.MyTaskFlowSections.Update(section);
        await db.SaveChangesAsync(cancellationToken);
        return section;
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<bool> DeleteAsync(Guid sectionId, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        var section = await db.MyTaskFlowSections
            .FirstOrDefaultAsync(candidate => candidate.SubscriptionId == subscriptionId && candidate.Id == sectionId, cancellationToken);

        if (section is null)
        {
            return false;
        }

        db.MyTaskFlowSections.Remove(section);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private void EnsureSubscriptionMatch(Guid entitySubscriptionId)
    {
        var currentSubscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        if (entitySubscriptionId != currentSubscriptionId)
        {
            throw new InvalidOperationException("Section subscription does not match current subscription context.");
        }
    }
}
