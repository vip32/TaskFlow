# TaskFlow Agent Instructions

This file is the working guide for coding agents in this repository.

## Project Overview

TaskFlow is a Blazor Server productivity app focused on helping users complete work, not just collect tasks. It supports project-based task management, rich task details (priority, notes, subtasks, focus markers), and cross-project "My Task Flow" views (Today, Upcoming, Recent).

The codebase follows DDD with a rich domain model and clean architecture. Business behavior belongs in domain entities, while the application layer orchestrates use-cases and the infrastructure layer handles persistence.

## Technology Stack

- Runtime and language: .NET 10, C# 14
- Web UI: Blazor Server (SignalR-backed interactivity)
- UI component library: MudBlazor 8.x
- Data access: Entity Framework Core
- Database: SQLite
- Deployment: Docker (Raspberry Pi ARM64 target), private Tailscale network
- Testing: xUnit

## Design References

- Visual UI/UX guide based on current brand/product screenshots: `docs/VISUAL_UI_UX_GUIDE.md`

## Layering Details

- **Dependency direction**: `Presentation -> Application -> Domain` and `Infrastructure -> Domain`
- **Domain layer**: Aggregates, entities, value types, invariants, repository interfaces
- **Application layer**: Orchestrators only (coordination, transactions, cross-aggregate flows), no business rules
- **Infrastructure layer**: EF Core `DbContext`, repository implementations, persistence configuration
- **Presentation layer**: Razor pages/components, UI state and interactions only

## Scope

- Apply these rules for all edits in this repo.
- Prefer small, focused changes that match existing conventions.
- Generate concise, idiomatic C# 10+ (.NET 10) code following DDD and clean architecture.
- Respect layering boundaries and module isolation; avoid cross-layer leakage.
- Prefer repository abstractions and specifications over direct DbContext access in Application code.
- Use existing devkit features (requester, notifier, pipeline behaviors) instead of re-inventing infrastructure.
- Produce testable changes with unit/integration tests where meaningful.


## Agent Skills

**IMPORTANT**: This project uses Agent Skills to provide specialized, standardized workflows for common tasks.

### Skills Usage Policy

- **ALWAYS check for and use available skills** when the user's request matches a skill's description.
- Skills are located in `.agents/skills/` directories.
- Each skill provides a tested, standardized approach to specific tasks.
- Using skills ensures consistency, follows best practices, and reduces errors.
- Use the `find-skills` skill to discover available skills when you're unsure which skill applies to your task.

### When to Use Skills

- When a user request explicitly matches a skill's purpose (e.g., "commit changes" → use `git-commit` skill)
- When performing tasks that have established workflows (e.g., adding aggregates, reviewing code)
- Before manually implementing any workflow, check if a skill exists for it
- **Default to using skills over ad-hoc manual approaches**

### Skill Priority

1. **First**: Check if a skill exists for the task
2. **Second**: Load and follow the skill's workflow
3. **Last Resort**: Only use manual approaches when no skill exists

This ensures all agents follow the same high-quality, tested patterns that the project relies on.

## Implementation Rules

- Please follow the rules in [.editorconfig](./.editorconfig).
- **Language**: C# 10+; file-scoped namespaces.
- **Style**: Follow C# Coding Conventions; descriptive names; expressive syntax (null-conditional, string interpolation).
- **Types**: Use `var` when type is obvious; prefer records, pattern matching, null-coalescing assignment.
- **Naming**:
  - PascalCase for classes, methods, public members.
  - camelCase for locals/private fields; prefix interfaces with `I` (e.g., `IUserService`).
  - Constants in UPPERCASE.
  - Use `this.` for fields.
- **Validation & Errors**: Prefer `Result<T>` for recoverable failures; exceptions only for exceptional cases. Use FluentValidation for inputs.
- **Async**: Use `async/await` for I/O-bound operations.
- **LINQ**: Prefer efficient LINQ; avoid N+1 queries.
- **Nullability**: Project uses disabled nullability annotations; maintain consistency.

- Follow existing naming and async patterns (`*Async` for async methods).
- Keep classes cohesive; avoid mixing UI, domain, and persistence concerns.
- Avoid broad refactors unless required for the requested change.
- Do not add new dependencies unless justified by the task.

## Architecture Guardrails

- Keep business logic in domain aggregates and entities (rich domain model).
- Adhere to Domain Driven Design principles.
- Keep application layer thin: orchestration, coordination, transactions.
- Use repositories for data access; keep `DbContext` usage in infrastructure.
- Do not introduce general-purpose DTO mapping between domain and UI/application.
- Import/export DTOs are allowed only for serialization workflows.

## Layer Responsibilities

- `TaskFlow.Domain`: entities, value objects, business rules, repository interfaces.
- `TaskFlow.Application`: orchestrators/use-case coordination.
- `TaskFlow.Infrastructure`: EF Core, repository implementations, persistence details.
- `TaskFlow.Presentation`: Blazor components/pages and UI interactions.

## Testing Expectations

- Add or update tests for non-trivial behavior changes.
- Prefer xUnit tests with clear Arrange-Act-Assert structure.
- Mock collaborators when unit testing orchestrators.
- Keep domain tests focused on business behavior.

## Documentation

- Use Markdown for docs located under `/docs/`.
- Keep `README.md` updated for any changes.

## Reference Docs

- Product and requirements: `docs/REQUIREMENTS.md`
- System design: `docs/SYSTEMDESIGN.md`
- Project overview: `README.md`
