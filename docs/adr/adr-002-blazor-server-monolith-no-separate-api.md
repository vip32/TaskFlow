# ADR-002: Build as a Blazor Server monolith without a separate API

**Status:** Accepted
**Date:** 2026-02-09
**Deciders:** TaskFlow maintainers
**Related ADRs:** [ADR-001](./adr-001-clean-architecture-rich-domain-model.md)

## Context

TaskFlow is a single-user application running in a private network, with a focus on rapid iteration and low operational overhead. Real-time UI updates are a core experience requirement.

The team wants to reduce boilerplate and avoid maintaining parallel contracts between UI and API layers for this deployment profile.

## Decision

Implement TaskFlow as a single Blazor Server application (monolith) and do not introduce a separate REST API layer at this stage.

Use application orchestrators and repositories directly from the server-side Blazor presentation layer. Keep real-time behavior via Blazor Server's SignalR model.

## Alternatives Considered

### Option A: Blazor WebAssembly + separate ASP.NET Core API

**Pros:**
- Clear client/server boundary
- Easier path to horizontal scaling for API workloads

**Cons:**
- Higher complexity (multiple deployable units and contracts)
- Additional API design, auth, and versioning work not needed yet

**Why not chosen:** Complexity is not justified for current single-user scope.

### Option B: MVC/Razor Pages server-rendered app

**Pros:**
- Simpler runtime model
- Mature pattern with broad ecosystem support

**Cons:**
- More manual work for rich interactive UX
- Weaker fit for real-time, component-driven interactions

**Why not chosen:** It provides less natural support for the target interactive behavior.

### Option C: Desktop application

**Pros:**
- Works without browser deployment concerns
- Potential offline-first behavior

**Cons:**
- Cross-device access becomes harder
- Distribution and update process is heavier

**Why not chosen:** Browser-based private access aligns better with deployment goals.

## Consequences

### Benefits

- Faster development with fewer moving parts.
- Real-time UI updates are straightforward using Blazor Server.
- Lower infrastructure and operational complexity.

### Drawbacks

- Tighter coupling between UI runtime and server availability.
- Future migration to public multi-user architecture may require splitting boundaries later.

### Risks

- If product scope expands significantly, architecture migration effort may increase.

### Trade-offs Accepted

We prioritize development speed and operational simplicity over early distributed-system boundaries.
