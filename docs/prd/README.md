# PRD Index

This folder contains feature-slice product requirement documents.

## Naming Convention

Use:

`prd-<ID>-<SLICE>-<title>.md`

- `<ID>`: zero-padded numeric ID (minimum 4 digits)
- Use slice-anchored ranges in steps of 100 (`0000`, `0100`, `0200`, ...)
- Add PRDs within each slice range (`0100`, `0101`, `0102`, ...)
- `<SLICE>`: uppercase slice code (`TASKS`, `PROJECTS`, `SETTINGS`, ...)
- `<title>`: lowercase kebab-case
- A slice can have one or more PRD files.
- Create multiple PRDs for the same slice when there is more than one distinct story or workflow.
- Keep each PRD focused on a coherent requirement scope.

### Slice Ranges

These ranges group PRDs by functional area in the application.  
Each range maps to one app capability boundary, so new workflows can be added without renumbering other slices.

- `0000-0099`: `SHELL` - app shell, home context, primary navigation scaffolding.
- `0100-0199`: `TASKS` - task lifecycle and task-level behaviors (create/edit/toggle/subtasks).
- `0200-0299`: `BOARD` - kanban board lanes, drag/drop, and lane-based interactions.
- `0300-0399`: `FLOW` - My Task Flow sections, selection model, and section semantics.
- `0400-0499`: `PROJECTS` - project management and project-specific preferences.
- `0500-0599`: `SUBSCRIPTION` - subscription boundary behavior (settings, timezone, schedules, activation state).
- `0600-0699`: `UX` - cross-cutting interaction patterns (shortcuts, feedback surfaces).
- `0700-0799`: `FOCUS` - focus timer workflows and session controls.
- `0800-0899`: `ABOUT` - product/about informational surfaces.
- `0900-0999`: `TAGS` - tag management workflows for tasks/projects.
- `1000-1099`: `EXCHANGE` - end-user import/export and data transfer workflows.
- `1100-1199`: `REPORTING` - analytics and reporting capabilities.
- `1200-1299`: `ACCESS` - authentication/authorization and access-control flows.

## Minimal Frontmatter (Required)

Every PRD must start with:

```yaml
---
id: PRD-<ID>
title: <Feature or Workflow Title>
slice: <SLICE>
status: Implemented | Partial | Pending
---
```

Examples:

- `prd-0100-TASKS-task-workspace-home.md`
- `prd-0300-FLOW-my-task-flow-sections.md`
- `prd-0400-PROJECTS-project-management.md`

## Story Status Legend

- `Implemented`: behavior exists and is currently available.
- `Partial`: behavior exists only in part (incomplete flow/surface).
- `Pending`: behavior is not yet implemented.

## Readiness Legend

- `Ready: Yes`: story satisfies mandatory Definition of Ready checks and has no critical INVEST failure in `V`, `S`, or `T`.
- `Ready: No`: story is missing one or more mandatory checks or has a critical INVEST failure.

## Definition of Ready

### Mandatory checks

1. Story name is clear, concise, and specific.
2. Story format is: `As [actor], I want [action], so that [achievement].`
3. Acceptance criteria clearly define what success looks like.
4. Notes capture dependencies/external input and known risks/constraints.

### Optional checks

1. Required assets/content/design links are attached when needed.
2. Story size estimate is captured.
3. Priority/order is explicit.

## Required Fields Per Story

- `Status: Implemented | Partial | Pending`
- `Ready: Yes | No`
- `Ready Reason: ...`
- `User Story: As..., I want..., so that...`
- `Acceptance Criteria: ...`
- `Notes: dependencies / external input / risks / constraints`
- `INVEST Check: I/N/V/E/S/T with short notes`

## Current PRD Inventory

- `PRD-0000` `SHELL` Home Navigation and Context Selection (`Implemented`)
- `PRD-0400` `PROJECTS` Project Lifecycle and Selection (`Implemented`)
- `PRD-0401` `PROJECTS` Project View Mode Preference (`Implemented`)
- `PRD-0100` `TASKS` Task Creation and Assignment (`Implemented`)
- `PRD-0101` `TASKS` Task Editing Inline and Dialog (`Implemented`)
- `PRD-0102` `TASKS` Task State and Quick Actions (`Implemented`)
- `PRD-0103` `TASKS` Task Filter Search Sort and Visibility (`Implemented`)
- `PRD-0104` `TASKS` Clear Completed and Undo (`Implemented`)
- `PRD-0105` `TASKS` Subtasks Management (`Implemented`)
- `PRD-0107` `TASKS` Task Completion and Uncompletion with Subtasks (`Implemented`)
- `PRD-0200` `BOARD` Board Lanes and Drag Drop (`Implemented`)
- `PRD-0300` `FLOW` Section Selection and Navigation (`Implemented`)
- `PRD-0301` `FLOW` Built In Section Semantics (`Implemented`)
- `PRD-0500` `SUBSCRIPTION` Always Show Completed Preference (`Implemented`)
- `PRD-0501` `SUBSCRIPTION` Time Zone Context and Local Date Bucketing (`Partial`)
- `PRD-0502` `SUBSCRIPTION` Activation Schedules and Active State (`Partial`)
- `PRD-0600` `UX` Keyboard Shortcuts and Help Dialog (`Implemented`)
- `PRD-0601` `UX` Snackbar and Error Feedback (`Implemented`)
- `PRD-0700` `FOCUS` Focus Timer Surface and Session Controls (`Implemented`)
- `PRD-0800` `ABOUT` About Page and Version Surface (`Implemented`)
- `PRD-0106` `TASKS` Due Date and Reminders UI (`Pending`)
- `PRD-0900` `TAGS` Task and Project Tags UI (`Pending`)
- `PRD-1000` `EXCHANGE` Project Import Export User Flows (`Pending`)
- `PRD-1100` `REPORTING` Analytics and Insights (`Pending`)
- `PRD-1200` `ACCESS` Authentication and Authorization (`Pending`)
