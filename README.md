# TaskFlow

TaskFlow is a focused productivity app built to help people do more than collect tasks - it helps them act on what matters and finish meaningful work.

It is designed around one core idea: move from task hoarding to task completion. The centerpiece is **My Task Flow** - a cross-project execution surface where tasks are triaged, scheduled, and completed.

## TL;DR

TaskFlow turns your task list into an execution system. Instead of burying tasks in static project lists, it helps you prioritize, triage, and complete work through **My Task Flow** sections like Recent, Today, This Week, and Upcoming.

## Concept

The core concept is **My Task Flow**: a cross-project execution view that helps users decide what to do now, what to schedule next, and what to process later.

Tasks can be created without a project, appear in Recent for triage, and be assigned/completed/cancelled directly from My Task Flow. Users can also create custom sections with hybrid behavior (rule-based + manual curation), so the view adapts to how they think and work.

Time-based grouping (Today, This Week, Upcoming) and reminders are evaluated in the subscription timezone (default `Europe/Berlin`) to keep planning behavior predictable.

## How Users Take Advantage

- Keep project structure for planning, while using My Task Flow for daily execution
- Capture fast with unassigned tasks, then triage in Recent
- Prioritize with rich task context: priority, notes, subtasks, tags, and focus markers
- Use built-in sections (Recent, Today, This Week, Upcoming) and custom hybrid sections
- Add multiple reminders per task (for example: 15 minutes before and on time)
- Switch between list and board views depending on workflow style
- Move and maintain selected projects via JSON import/export with ID-based add/update

## Features Overview

- **My Task Flow (primary feature)**
  - Cross-project execution surface with built-in sections: Recent, Today, This Week, Upcoming
  - Custom user-managed sections with hybrid population (rules + manual curation)
  - Unassigned task triage directly from flow (assign, complete, cancel)
- Project-based task organization with clear ownership and context
- Rich tasks with priority, notes, subtasks, tags, and focus markers
- Due dates and multiple reminders per task
- Subscription-timezone aware scheduling (default `Europe/Berlin`)
- List and board workflows to match different planning styles
- Focus-oriented workflow support for action and momentum
- Import/export for selected projects using JSON (ID-based add/update)

## Target Audience

TaskFlow is for people who already have long task lists but struggle to consistently complete what matters most. It is especially useful for:

- Solo professionals and small business owners juggling multiple priorities
- Students managing assignments, deadlines, and research tasks
- Parents and households coordinating many parallel responsibilities
- Anyone who wants a practical, execution-first task system instead of a passive backlog

## Architectural Overview

TaskFlow follows a layered architecture with a rich domain model at its core.

- `TaskFlow.Presentation`: Blazor Server UI, routing, and dependency registration
- `TaskFlow.Application`: application services/orchestrators for use-case coordination
- `TaskFlow.Domain`: core aggregates, value types, and business behavior
- `TaskFlow.Infrastructure`: persistence and external integrations
- `TaskFlow.UnitTests`: xUnit-based tests using AAA and Shouldly assertions

Key principle: business rules live in the domain, while application services orchestrate and repositories handle database access.

## Release Process

TaskFlow uses tag-driven releases with GitHub Actions and MinVer.

### Prerequisites

- Keep release notes up to date in `CHANGELOG.md`.
- Ensure `master` is green in the `Build` workflow.

### How a Release Is Created

1. Add a new section in `CHANGELOG.md` (for example `## [1.0.2] - YYYY-MM-DD`) and commit it.
2. Create and push a stable SemVer tag:

```bash
git tag 1.0.2
git push origin 1.0.2
```

3. Pushing the tag triggers `.github/workflows/release.yml`, which:
   - restores tools and resolves version with `dotnet minver`
   - validates/builds/tests the solution in Release mode
   - creates a GitHub Release with generated release notes

### Tag Rules

- Stable tags must match `X.Y.Z` (for example `1.0.2`) to publish a release.
- Pre-release tags like `1.1.0-beta.1` or `1.1.0-preview.1` do not create a stable GitHub release.
- MinVer is configured for plain tags (no `v` prefix).
