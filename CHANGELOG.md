# Changelog

All notable updates to TaskFlow are documented here.

## [Unreleased]

### Added
- Subscription domain model baseline with schedules and tiers (`Free`, `Plus`, `Pro`) plus current subscription accessor.
- Infrastructure persistence baseline: `AppDbContext`, repositories, initial migration, and initial data seeding.
- Application orchestration baseline: `IProjectOrchestrator` and `ITaskOrchestrator` with immediate-persistence operations.
- New domain and orchestrator unit tests (13 tests total passing).
- Task history (`TaskHistory`) and repository for autocomplete/autofill of previously used task and subtask names.
- Tagging support in domain aggregates for projects and tasks (subtasks inherit task behavior).
- My Task Flow domain foundation with section aggregates, due-date buckets, manual section membership links, and section orchestration contracts.
- Task reminder domain support with multiple reminders per task and two modes (`RelativeToDueDateTime`, `DateOnlyFallbackTime`).
- New My Task Flow and task domain test coverage (`TaskDomainTests`, `MyTaskFlowSectionTests`, `MyTaskFlowSectionOrchestratorTests`).
- Additional local tool manifest entry for `csharp-ls` in `dotnet-tools.json`.
- Persisted ordering support for project tasks and subtasks with new reorder use cases (`FR2.10`, `FR3.6`) and traceability updates.
- EF Core migration `AddTaskOrdering` for persisted `Task.SortOrder` and ordering index improvements.
- Optional-field nullability model for domain entities (for example unassigned task `ProjectId`, optional completion/due/reminder timestamps, and open-ended subscription schedules).
- New single baseline migration `InitialCreate` regenerated from the current domain model after migration reset.

### Changed
- Subscription schedules now use `DateOnly` to avoid UTC date boundary issues.
- Repository contracts and implementations are scoped through current subscription context accessor.
- Presentation startup now wires both application and infrastructure dependency registration.
- Presentation logging switched to Serilog with console and rolling file outputs.
- Task workflow statuses now include `New`, `InProgress`, `Paused`, `Done`, and `Cancelled`.
- Dependency injection for obvious services is now attribute-driven with Injectio source generation.
- Subscription now stores timezone (`TimeZoneId`) with default `Europe/Berlin`; current subscription accessor and seed data align to this default.
- Task aggregate now supports unassigned tasks, assign/unassign behaviors, due date/date-time fields, Today marker, and reminder lifecycle operations.
- Task orchestrator and repository interfaces were expanded for My Task Flow retrieval (`Recent`, `Today`, `This Week`, `Upcoming`) and due-date based queries.
- EF Core model and migration snapshot now include My Task Flow section tables, reminder table, and additional task scheduling indexes.
- Unit test suite increased to 26 passing tests after new domain/orchestrator coverage.
- Task aggregate now includes persisted `SortOrder`; task and subtask creation paths assign next sibling order value.
- Task orchestration and repository contracts now support project/subtask reordering and ordered retrieval.
- Unit test suite increased to 31 passing tests after ordering coverage and domain invariant checks.
- Sentinel values (`Guid.Empty`, `DateTime.MinValue`, `DateOnly.MinValue`, `TimeOnly.MinValue`) were replaced by nullable optional properties where semantically appropriate.
- EF Core mappings were updated so optional fields are nullable in persistence, including optional task-project foreign key behavior.
- Historical incremental migrations were consolidated by deleting prior migration chain and recreating a single initial migration from current schema.

## [1.0.1] - 2026-02-09

### Added
- New release workflow at `.github/workflows/release.yml` that uses MinVer and creates GitHub releases for stable tags.
- Additional local .NET tools in `dotnet-tools.json`: `dotnet-ef`, `dotnet-outdated-tool`, and `dotnet-inspect`.

### Changed
- MinVer tag prefix handling was updated to use plain tags (for example `1.0.1` instead of `v1.0.1`).
- Docker build flow was updated to copy central props files before restore and run publish with restore for reliable CI container builds.

## [1.0.0] - 2026-02-09

### Added
- Skill-driven automation support under `.agents/skills`, including workflows for development and release operations.
- A dedicated changelog generation skill to turn commit history into user-friendly release notes.
- Initial solution and project structure with clean architecture layers under `src/` and tests under `tests/`.
- New projects: `TaskFlow.Domain`, `TaskFlow.Application`, `TaskFlow.Infrastructure`, `TaskFlow.Presentation`, and `TaskFlow.UnitTests`.
- Rich domain model baseline: `Project`, `Task`, `FocusSession`, value enums, and repository interfaces.
- CI workflow at `.github/workflows/build.yml` for restore, build, test, and container build (no image push).
- Root `Dockerfile` for building and running the Blazor presentation app.
- Local tool manifest `dotnet-tools.json` with `minver-cli`.
- Central package management via `Directory.Packages.props`.
- Shared build settings and MinVer wiring via `Directory.Build.props`.
- XML documentation comments for all current public domain symbols (classes, enums, properties, and methods).

### Changed
- Documentation structure was reorganized into a dedicated `docs/` area for easier navigation and maintenance.
- README messaging was refreshed to better explain TaskFlow's value, audience, and architecture.
- Project configuration now enforces `net10.0`, `LangVersion=latest`, nullable disabled, and warnings treated as errors.
- System design and planning documents were updated to reflect the repository's actual implemented baseline and progress tracking.

### Documentation
- Initial architecture and delivery plan documentation was added to establish project direction.
- Changelog scaffold was introduced and is now populated with the first project updates.
