---
id: PRD-0302
title: My Task Flow Project Navigation
slice: FLOW
status: Pending
---

# Product Requirements: My Task Flow Project Navigation

## Overview
Defines project name chips on tasks in My Task Flow views, enabling quick navigation from cross-project task lists to individual project context.

## Scope
- In scope: Project chip display on My Task Flow tasks, click-to-navigate behavior.
- Out scope: Project filtering within My Task Flow, project creation from My Task Flow.

## Diagram
```text
My Task Flow (e.g., Today section):
  Task Title 1          [Project A Chip]
                                ^
                                | click
                                +--> Navigate to Project A task view

  Task Title 2          [Project B Chip]
  Task Title 3          (unassigned - no chip)
```
Covers AC: 1-3

## User Roles
- Primary: TaskFlow subscription user

## Stories

### Story 1: Navigate from task to project context
- Status: Pending
- Ready: Yes
- Ready Reason: DoR checks satisfied with clear navigation affordance.
- User Story: As a user, I want to quickly navigate from a task in My Task Flow to its project, so that I can jump into detailed context.

Acceptance Criteria:
1. When My Task Flow displays tasks, each task with a project assignment shows a chip with the project name.
2. When I click the project name chip, I navigate to that project's task view.
3. When a task is not assigned to a project, no project chip is displayed.

Data Requirements:
- Task-project relationship: `Task.ProjectId` (existing FK).
- Chip display: `Project.Name` and optional `Project.Color` for styling.

Notes:
- Dependencies / external input: My Task Flow task rendering components, `AppState.SetNavigationData`.
- Risks / constraints: Chip click should not preserve My Task Flow filters; navigation switches context to project.
- Technical context: Navigation sets `activeSection = null`, `selectedProject = clicked project`, and navigates to `/`.

INVEST Check:
- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Pass
- T: Pass

## Non-Functional Notes
- Accessibility: Project chips should be keyboard-focusable with clear focus indication.
- Visual consistency: Chip styling should match existing project color/icon patterns.
- Performance: Chip rendering should not significantly impact My Task Flow load time with 100+ tasks.

## Open Questions
- None.

## Traceability
- Extends: `PRD-0300` (Section Selection) - adds navigation context to My Task Flow display.
- Uses: `PRD-0400` (Project Lifecycle) - navigates to existing project view.
