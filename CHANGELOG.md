# Changelog

All notable updates to TaskFlow are documented here.

## [Unreleased]

### Added
- Subscription domain model baseline with schedules and tiers (`Free`, `Plus`, `Pro`) plus current subscription accessor.
- Infrastructure persistence baseline: `AppDbContext`, repositories, initial migration, and initial data seeding.
- Application orchestration baseline: `IProjectOrchestrator` and `ITaskOrchestrator` with immediate-persistence operations.
- New domain and orchestrator unit tests (13 tests total passing).

### Changed
- Subscription schedules now use `DateOnly` to avoid UTC date boundary issues.
- Repository contracts and implementations are scoped through current subscription context accessor.
- Presentation startup now wires both application and infrastructure dependency registration.
- Presentation logging switched to Serilog with console and rolling file outputs.

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
