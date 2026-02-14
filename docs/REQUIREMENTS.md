# TaskFlow Requirements (Current Implementation)

## 1. Document Overview

### Purpose
This document describes the requirements that match the current, running TaskFlow implementation.

### Version
- Version: 2.3
- Date: February 12, 2026
- Status: Implemented Baseline

### Validation Method
The UI scope in this document was validated against the running Blazor Server app using Playwright on February 12, 2026.
Validated routes:
- `/`
- `/settings`
- `/focus-timer`
- `/about`

### PRD Decomposition
Detailed workflow-level requirements are documented in `docs/prd/` as PRDs using slice-based 100 ranges (`PRD-0000`, `PRD-0100`, `PRD-0200`, ...).
This baseline remains the canonical high-level requirements source, while PRDs provide workflow detail.

---

## 2. Product Scope

TaskFlow is a subscription-scoped productivity app with:
- Project-based task management
- Cross-project "My Task Flow" sections
- List and Board task views
- Inline editing and task quick actions
- Undo-capable destructive actions
- Focus timer and basic settings pages

---

## 3. Functional Requirements (Implemented)

## FR1. Subscription Boundary

### FR1.1 Subscription Ownership
- All core data is scoped to one active subscription context.
- Subscription owns projects, tasks, my-task-flow sections, focus sessions, and subscription settings.

### FR1.2 Subscription Settings
- Subscription has a dedicated settings entity.
- Current implemented setting:
  - `AlwaysShowCompletedTasks` (bool)
- Setting is persisted and editable from `/settings`.
- When enabled, completed tasks are always shown in both list and board views.

## FR2. Project Management

### FR2.1 Project CRUD
- User can create projects from the sidebar.
- User can edit project details via dialog.
- User can delete non-default projects.
- Deleting a project supports undo.

### FR2.2 Project Navigation
- Sidebar shows:
  - My Task Flow sections
  - Projects list
  - Per-project task count badge
- Active project/section is highlighted.

### FR2.3 Project View Type
- Each project persists its preferred view type:
  - `List`
  - `Board`
- User can toggle view type from Home top bar when a project is selected.

## FR3. Task Management

### FR3.1 Create Task
- Task can be created from Home input.
- Tasks can be project-assigned or unassigned.
- Default priority is Medium.

### FR3.2 Edit Task
- Task title supports inline editing in list and board rows.
- Additional edit operations are available in task edit dialog (opened via edit action).

### FR3.3 Task State and Metadata
- Implemented task toggles/actions:
  - Complete/incomplete
  - Focus pin
  - Important star
  - Mark for today
- Implemented task mutation actions:
  - Duplicate
  - Move between projects
  - Delete
- Deleting a task supports undo.

### FR3.4 Clear Completed
- User can clear all completed tasks in current selection.
- Operation supports undo.

### FR3.5 Filtering, Search, Sort
- Search by task title/note.
- Filters:
  - Priority
  - Status
  - Show completed
- Sort modes:
  - Newest
  - Oldest
  - Priority
  - Focused

## FR4. Board View

### FR4.1 Board Lanes
- Board has three lanes:
  - `Todo`
  - `Doing`
  - `Done`
- Lane headers show task counts.

### FR4.2 Board Interactions
- Drag-and-drop task between lanes updates status.
- Task can be created directly in a specific lane.
- Lanes are shown side-by-side with equal width in desktop layout.

### FR4.3 Completed Visibility in Board
- Board completed visibility follows subscription setting + local show-completed toggle behavior.
- If `AlwaysShowCompletedTasks` is enabled, completed tasks stay visible in board and list regardless of local toggle.

## FR5. Subtasks

### FR5.1 Subtask Support
- Subtasks are supported (one level).
- Subtasks are managed from task edit dialog.
- Subtask create/edit/complete/delete flows are implemented through the same task orchestration patterns.

## FR6. My Task Flow Sections

### FR6.1 Built-in Sections
- Implemented system sections:
  - Recent
  - Today
  - Important
  - This Week
  - Upcoming

### FR6.2 Section Selection
- User can switch between section-based and project-based views from sidebar.

## FR7. Undo and Notifications

### FR7.1 Undo Manager
- Undo manager tracks reversible actions.
- Implemented undo trigger:
  - `Ctrl/Cmd + Z`

### FR7.2 Toast Notifications
- Snackbar/toast notifications are shown for success/info/warning/error outcomes.

## FR8. Keyboard Shortcuts

### FR8.1 Implemented Shortcuts
- `Ctrl/Cmd + Enter`: complete selected task
- `Delete`: delete selected task
- `Ctrl/Cmd + N`: focus new-task input
- `Ctrl/Cmd + F`: focus search input
- `Ctrl/Cmd + Z`: undo last action
- `Ctrl/Cmd + /`: open shortcuts dialog

## FR9. Settings and Support Pages

### FR9.1 Settings Page
- `/settings` contains subscription preference UI for:
  - Always show completed tasks in list and board

### FR9.2 Focus Timer Page
- `/focus-timer` route exists with focus timer UI surface.

### FR9.3 About Page
- `/about` route exists with product summary and version display.

---

## 4. Non-Functional Requirements (Implemented Baseline)

## NFR1. Architecture
- Layered architecture:
  - Presentation -> Application -> Domain
  - Infrastructure -> Domain
- Domain contains business behavior and invariants.
- Application layer coordinates use cases.
- Infrastructure handles EF Core persistence.

## NFR2. Technology
- Runtime: .NET 10
- UI: Blazor Server + MudBlazor
- Data: EF Core + SQLite
- Tests: xUnit + Shouldly + NSubstitute

## NFR3. Responsiveness
- Sidebar is responsive and togglable.
- Board lane layout uses equal-width lanes and avoids lane wrapping in desktop layout.

## NFR4. Data Persistence
- Core entities are persisted with EF Core migrations.
- Subscription settings are persisted in `SubscriptionSettings` table.

---

## 5. Current Gaps / Not Yet Implemented in UI

The following capabilities exist partially in domain/application but are not fully exposed as end-user UI workflows today:
- Task due date and reminder management UI
- Task/project tag management UI
- Full import/export user flows in current UI
- Advanced analytics/reporting
- Multi-user authentication/authorization

These items remain valid candidates for future enhancement requirements.

---

## 6. Constraints

- Single subscription context is currently assumed in runtime configuration.
- No authentication boundary in current app UX.
- SQLite is the active persistence backend.

---

## 7. Acceptance Baseline

A build is acceptable when:
- `dotnet build TaskFlow.slnx -c Debug` passes.
- `dotnet test tests/TaskFlow.UnitTests/TaskFlow.UnitTests.csproj --configuration Debug` passes.
- Coverage collection command passes with configured `coverlet.runsettings`.
- Key UI routes load and core interactions function:
  - `/` (task list/board operations)
  - `/settings` (subscription setting save)
  - `/focus-timer`
  - `/about`

---

## 8. PRD Coverage Mapping

Implemented workflow PRDs:
- `PRD-0000`
- `PRD-0100` to `PRD-0105`
- `PRD-0107`
- `PRD-0200`
- `PRD-0300` to `PRD-0301`
- `PRD-0400` to `PRD-0401`
- `PRD-0500`
- `PRD-0600` to `PRD-0601`
- `PRD-0700`
- `PRD-0800`

Partial workflow PRDs:
- `PRD-0501` to `PRD-0502`

Pending workflow PRDs:
- `PRD-0106`
- `PRD-0302`
- `PRD-0402`
- `PRD-0900`
- `PRD-1000`
- `PRD-1100`
- `PRD-1200`

Reference documents:
- PRD index: `docs/prd/README.md`
- Traceability matrix: `docs/prd/TRACEABILITY.md`

---

## 9. Version History

| Version | Date | Changes |
|---|---|---|
| 2.4 | 2026-02-13 | Added workflow-level PRD decomposition in `docs/prd/` (implemented and pending slices), added traceability matrix at `docs/prd/TRACEABILITY.md`, aligned baseline to hybrid requirements model |
| 2.3 | 2026-02-12 | Replaced speculative requirements with current implemented baseline; added subscription settings requirement (`AlwaysShowCompletedTasks`); aligned board/list behavior and page scope with actual app |
| 2.2 | 2026-02-09 | Added persisted task/subtask ordering requirements |
| 2.1 | 2026-02-09 | Expanded My Task Flow and due/reminder requirement set |
| 2.0 | 2025-02-09 | Added architecture/code quality requirement sections |









































