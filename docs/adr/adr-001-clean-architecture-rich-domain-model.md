# ADR-001: Use Clean Architecture with a rich domain model

**Status:** Accepted
**Date:** 2026-02-09
**Deciders:** TaskFlow maintainers
**Related ADRs:** None

## Context

TaskFlow needs to stay easy to evolve while adding features such as board workflows, import/export, and smart views. The project also has strong requirements for testability, clear boundaries, and keeping business rules in one place.

The team wants to avoid an anemic domain model where rules spread across UI, services, and persistence code. For long-term maintainability, behavior should be expressed as domain methods and protected by aggregate boundaries.

## Decision

Adopt a four-layer Clean Architecture model across the solution:

- `TaskFlow.Domain`: aggregates, value objects/enums, repository interfaces, and business behavior
- `TaskFlow.Application`: orchestration and cross-aggregate coordination
- `TaskFlow.Infrastructure`: EF Core and repository implementations
- `TaskFlow.Presentation`: Blazor UI and interaction logic

Use a rich domain model where domain entities own business rules, while application orchestrators stay thin and coordinate workflows.

## Alternatives Considered

### Option A: Layered architecture with richer service layer

**Pros:**
- Familiar to most .NET developers
- Keeps UI simpler by centralizing behavior in services

**Cons:**
- Business logic can drift into service classes and become duplicated
- Weaker aggregate boundaries and invariants over time

**Why not chosen:** It increases risk of service-heavy, anemic models and weaker domain consistency.

### Option B: Feature-first architecture with minimal domain model

**Pros:**
- Fast for early delivery
- Low initial design overhead

**Cons:**
- Business rules become scattered across feature handlers and UI
- Harder to test domain behavior independently of infrastructure

**Why not chosen:** It optimizes short-term speed but creates higher long-term maintenance cost.

### Option C: Event-sourced domain model

**Pros:**
- Strong auditability and time-travel debugging
- Flexible read model projections

**Cons:**
- Significant complexity for a single-user personal product
- Higher implementation and operational overhead

**Why not chosen:** It is over-engineering for current scale and product needs.

## Consequences

### Benefits

- Domain behavior is explicit, testable, and centralized.
- Boundaries between UI, orchestration, and persistence remain clear.
- New features can be added with lower coupling and better readability.

### Drawbacks

- More upfront structure and conventions to maintain.
- Some flows require extra coordination code in orchestrators and repositories.

### Risks

- Team may accidentally move logic back into UI or infrastructure if guardrails are not enforced.

### Trade-offs Accepted

We accept a bit more structural complexity in exchange for stronger maintainability, testability, and domain correctness.
