# ADR-003: EF Core persistence strategy (SQLite + DbContextFactory repository pattern)

**Status:** Accepted
**Date:** 2026-02-09
**Deciders:** TaskFlow maintainers
**Related ADRs:** [ADR-001](./adr-001-clean-architecture-rich-domain-model.md), [ADR-002](./adr-002-blazor-server-monolith-no-separate-api.md)

## Context

TaskFlow is a Blazor Server application designed for container deployment with moderate data volume. The data model is relational and requires transactional consistency, simple operations, and straightforward backup/restore.

Blazor Server circuits can be long-lived, so DbContext lifetime management must avoid stale tracking state, cross-call state leaks, and memory growth.

The persistence strategy must therefore cover both:

- **Database choice** appropriate for current scale and operations.
- **DbContext lifetime pattern** safe for Blazor Server repository access.

## Decision

Adopt a single EF Core persistence strategy with these mandatory elements:

1. Use **SQLite** as the primary datastore.
2. Use **EF Core** for ORM, mapping, and migrations.
3. Register and use **`IDbContextFactory<AppDbContext>`**.
4. Repositories create/dispose DbContext **per method call** (`await using`).
5. Repositories are **stateless** and represent **single-unit CRUD/query operations**.
6. Reads use **`AsNoTracking()`** by default unless tracked updates are explicitly required.

`DbContext` is not injected into Blazor components. For multi-step transactional flows that span multiple repository calls, transaction ownership moves to an application-level orchestrator/service that uses `IDbContextFactory<AppDbContext>`.

## Alternatives Considered

### Option A: PostgreSQL + scoped DbContext injection

**Pros:**
- Strong scalability and advanced operational features
- Familiar production setup for multi-user workloads

**Cons:**
- Additional database service and operational overhead
- Scoped DbContext in Blazor Server can align with long-lived circuit lifetimes

**Why not chosen:** Overly heavy for current single-user scope and less safe by default for Blazor Server context lifetime.

### Option B: SQLite + direct scoped AppDbContext in repositories/components

**Pros:**
- Minimal boilerplate
- Familiar for many ASP.NET Core examples

**Cons:**
- Higher risk of stale tracked entities and implicit cross-call state
- Encourages DbContext leakage into UI layer

**Why not chosen:** Does not enforce safe lifecycle boundaries required by Blazor Server.

### Option C: JSON/file-based persistence without EF Core

**Pros:**
- Very simple runtime model
- Human-readable files

**Cons:**
- Weak relational guarantees and query ergonomics
- Harder to evolve safely as features grow

**Why not chosen:** Fails relational integrity and maintainability goals.

## Consequences

### Benefits

- Low operational burden with SQLite for current scale.
- Clear and safe DbContext lifecycle behavior for Blazor Server.
- Predictable repository behavior and better testability.
- Easier backup/restore through a single database file in persisted storage.

### Drawbacks

- More repository boilerplate due to per-method context creation.
- Multi-repository transactional operations require explicit orchestration code.
- SQLite limits future high-concurrency multi-user growth.

### Risks

- Team members may bypass the pattern and inject DbContext directly unless reviewed.
- Future scale changes may require migration to PostgreSQL.

### Trade-offs Accepted

We accept additional repository ceremony and reduced large-scale database capability in exchange for operational simplicity and safer Blazor Server data access behavior.

## Implementation

### Required DI registration pattern

```csharp
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
```

### Required repository pattern

```csharp
public sealed class TaskRepository : ITaskRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public TaskRepository(IDbContextFactory<AppDbContext> factory)
        => _factory = factory;

    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Tasks
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task AddAsync(TaskItem item, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        db.Tasks.Add(item);
        await db.SaveChangesAsync(ct);
    }
}
```

### Explicit non-goals

- No long-lived DbContext (especially not per Blazor circuit)
- No DbContext injected into components
- No repositories calling other repositories
- No implicit transactions across repository methods

### Escalation rule

If an operation requires multiple repository calls in one transaction:

- Use an application service/orchestrator that creates one DbContext via factory.
- Start and manage one explicit transaction there.
- Keep repositories simple and single-purpose.
