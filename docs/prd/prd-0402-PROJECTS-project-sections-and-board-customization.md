---
id: PRD-0402
title: Project Sections and Board Customization
slice: PROJECTS
status: Pending
---

# Product Requirements: Project Sections and Board Customization

## Overview

Defines project-specific sections that replace global Todo/Doing/Done lanes, enabling flexible board and list views. Sections are per-project, user-defined, and optional.

## Scope
- In scope: Project section management, project description, board view with custom sections, list view with collapsible section headers.
- Out scope: Section templates beyond Todo/Doing/Done, cross-project section sharing, section analytics.

## Diagrams

### Section Management Flow

```text
Create/Edit Project Dialog
  |
  +--> Use Template? [Optional]
  |     +--> Yes -> Create Todo/Doing/Done sections
  |     +--> No -> No sections (empty)
  |
  +--> Edit Sections Button
        +--> Section Management Dialog
              +--> Add Section (Name, Icon, Description optional)
              +--> Edit Section (Name, Icon, Description)
              +--> Delete Section (confirm, tasks -> Uncategorized)
              +--> Reorder Sections (drag or arrows)
```

Covers AC: Story 1.1-1.6

### Board View Flow

```text
When project selected with sections:
  [Section 1]  [Section 2]  [Section 3]  [...Uncategorized]
       ^             ^             ^
       |             |             |
       +-------------+-------------+ (drag-drop tasks)

When project selected with NO sections:
  [Prompt Message: "Create sections to use board view"]
  [Button: "Manage Sections"]
```

Covers AC: Story 2.1-2.4

### List View Flow

```text
Project List View with Sections:
  [▼ Section 1] (count)
      Task A
      Task B
  [▶ Section 2] (collapsed, count)
  [▼ Uncategorized] (count)
      Task C (no section)
```

Covers AC: Story 3.1-3.4

### Task Placement Rules

```text
Main "Add Task" input (top of page):
  Input -> Task assigned to First Section (by SortOrder)

Board Lane input (per-section "Add task"):
  Input in Section X -> Task assigned to Section X

List View "Add Task" input (top of page):
  Input -> Task assigned to First Section (regardless of expanded state)

Project with NO sections:
  Input -> Task assigned to "Uncategorized" bucket

Edit Dialog (when sections exist):
  Section dropdown appears -> User can change section assignment

Section Deletion:
  Delete Section X -> All tasks in X move to "Uncategorized"
```

Covers AC: Task placement across all stories

## User Roles

- Primary: TaskFlow subscription user

## Stories

### Story 1: Manage project sections

- Status: Pending
- Ready: Yes
- Ready Reason: DoR checks satisfied with clear section lifecycle and migration rules.
- User Story: As a user, I want to define custom sections per project, so that my board reflects my workflow.

Acceptance Criteria:

1. When creating a new project, I can optionally apply "Todo/Doing/Done" template.
2. When no template is applied, project starts with no sections.
3. When I manage sections, I can add, edit, delete, and reorder sections.
4. When I add a section, it requires name and icon; description is optional.
5. When I delete a section, all its tasks move to "Uncategorized" bucket.
6. When I edit project, I can add/update description text.
7. When project has no sections, new tasks are assigned to "Uncategorized" bucket.
8. When I edit a task and project has sections, a section dropdown appears allowing section assignment change.

Data Requirements:

- `ProjectSection` entity: `Id`, `ProjectId`, `Name`, `Icon`, `Description?`, `SortOrder`.
- `Project.Description` field (string, nullable).
- Relationship: `Project` 1 → N `ProjectSection`.
- Relationship: `Task` N → 1 `ProjectSection` (nullable).

Notes:

- Dependencies / external input: `ProjectEditDialog.razor`, `ProjectSectionManagementDialog.razor`, `ProjectOrchestrator`.
- Risks / constraints: Section names must be unique per project; "Uncategorized" is reserved system name.
- Technical context: Requires new `ProjectSection` aggregate root or entity within project aggregate.

INVEST Check:

- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Pass
- T: Pass

### Story 2: Use project sections in board view

- Status: Pending
- Ready: Yes
- Ready Reason: DoR checks satisfied with explicit board rendering and prompt behavior.
- User Story: As a user, I want board lanes to match my project sections, so that visual workflow matches my mental model.

Acceptance Criteria:

1. When board mode is active and project has sections, lanes display as configured sections plus "Uncategorized".
2. When I drag a task between section lanes, task's `ProjectSectionId` updates accordingly.
3. When I create a task in a lane, task is assigned to that section.
4. When I create a task via main "Add task" input, it is assigned to the first section.
5. When board mode is active and project has NO sections, a prompt message displays encouraging section creation.

Data Requirements:

- Board lane source: `Project.ProjectSections` ordered by `SortOrder`.
- "Uncategorized" lane: Tasks where `ProjectSectionId` is null.

Notes:

- Dependencies / external input: `TaskBoardView.razor`, board drag-drop handlers.
- Risks / constraints: Board view unavailable for projects without sections; list view remains functional.
- Technical context: Replace hardcoded `Todo/Doing/Done` with dynamic section rendering from project.

INVEST Check:

- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Pass
- T: Pass

### Story 3: Use project sections in list view

- Status: Pending
- Ready: Yes
- Ready Reason: DoR checks satisfied with clear collapsible header behavior.
- User Story: As a user, I want list view organized by collapsible section headers, so that I can scan focused work areas.

Acceptance Criteria:

1. When list mode is active and project has sections, tasks are grouped by section with collapsible headers.
2. When section header is expanded, tasks in that section are visible.
3. When section header is collapsed, tasks in that section are hidden but count remains visible.
4. When I create a task via main "Add task" input, it is assigned to the first section regardless of which section is expanded.
5. When tasks are not assigned to a section, they appear under "Uncategorized" header.

Data Requirements:

- List rendering: Group `Task` entities by `ProjectSectionId`, with null group as "Uncategorized".
- Section expansion state: Optional per-project or global user preference.

Notes:

- Dependencies / external input: `TaskListView.razor`, collapsible group headers.
- Risks / constraints: All sections collapsed initially or per-user preference? Default: all expanded.
- Technical context: Requires grouping logic in task query or UI layer.

INVEST Check:

- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Pass
- T: Pass

## Non-Functional Notes

- Performance: Board and list view rendering with sections should remain responsive with 100+ tasks.
- Accessibility: Section headers should be keyboard-navigable with ARIA expanded/collapsed state.
- Consistency: Section visual styling (color, icon) should apply to both board lanes and list headers.

## Open Questions

- Should section expansion state be per-project setting or global user preference?
- Should "Uncategorized" section be reorderable or always last?
- Should section descriptions appear as tooltips in headers?

## Traceability
- Affects: `PRD-0200` (Board Lanes) - extends with dynamic sections.
- Affects: `PRD-0400` (Project Lifecycle) - extends with description and sections.
- Affects: `PRD-0401` (Project View Mode) - board view behavior changes with sections.
