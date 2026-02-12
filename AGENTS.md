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
- Deployment: Docker containers in public cloud (Azure target)
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

## Frontend Guidelines

For Blazor frontend bug fixes and new features, follow this short process:

1. Reproduce first:
   - Verify the issue/behavior in the running app before editing code (prefer Playwright for repeatable repro).
2. Trace ownership:
   - Identify which component renders the UI and which parent/service/orchestrator owns the state change.
3. Patch minimally:
   - Make the smallest focused change that fixes the behavior.
   - Prefer MudBlazor-native components/patterns over custom HTML/CSS.
4. Use callbacks for child-to-parent actions:
   - Prefer `EventCallback` from child components; let parent pages orchestrate state changes.
5. Keep boundaries clean:
   - UI logic stays in Presentation.
   - Business rules stay in Domain/Application, not in Razor component markup.
6. Validate every change:
   - Run `dotnet build` for compile safety.
   - Run targeted headless Playwright checks for the changed UX flow.
   - Use the `playwright-skill` (`.agents/skills/playwright-skill/SKILL.md`) for browser automation workflow.
7. Report outcome clearly:
   - List changed files, behavior fixed, and validation steps/results.

## Backend Guidelines

For server-side development and bug fixes, follow this  workflow:

1. Start from behavior and use-case:
   - Define the expected business behavior first (inputs, rules, outputs, side effects).
2. Place logic in the correct layer:
   - `Domain`: business rules, invariants, entity behavior.
   - `Application`: orchestration/use-case flow, transactions, coordination between repositories/services.
   - `Infrastructure`: EF Core persistence, repository implementations, external integrations.
   - `Presentation`: transport/UI endpoint concerns only (no business rules).
3. Keep dependency direction strict:
   - `Presentation -> Application -> Domain`, and `Infrastructure -> Domain`.
   - Do not introduce reverse or cross-layer shortcuts.
4. Prefer domain-first fixes:
   - If a bug is a rule/invariant issue, fix it in Domain.
   - If a bug is coordination/order-of-operations, fix it in Application.
   - If a bug is query/persistence mapping, fix it in Infrastructure.
5. Repository discipline:
   - Use repository abstractions in Application.
   - Keep `DbContext` usage and EF-specific logic in Infrastructure only.
6. Validate safely:
   - Build after changes.
   - Add/update xUnit tests at the layer where behavior changed (especially Domain/Application).
7. Report by layer:
   - Summarize what changed in each impacted layer and why.

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
