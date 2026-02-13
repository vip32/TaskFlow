---
id: PRD-0105
title: Subtasks Management
slice: TASKS
status: Implemented
---

# Product Requirements: Subtasks Management

## Overview
Defines one-level subtask creation and management under a parent task.

## Scope
- In scope: Create/edit/complete/delete subtasks from task edit dialog.
- Out of scope: Multi-level nesting.

## User Roles
- Primary: TaskFlow subscription user

## Stories
### Story 1: Manage one-level subtasks
- Status: Implemented
- Ready: Yes
- Ready Reason: DoR checks satisfied with bounded parent-child behavior.
- User Story: As a user, I want subtasks under a task, so that I can track finer-grained progress.

Acceptance Criteria:
1. When I add subtask in edit dialog, child task linked by parent ID is created.
2. When I update subtask state or title, changes persist.
3. When I delete subtask, it is removed from parent subtask list.
4. When dialog loads, subtasks are listed for the selected parent.

Data Requirements:
- Parent-child link via `ParentTaskId`.

Notes:
- Dependencies / external input: `TaskEditDialog.razor`, `TaskOrchestrator.GetSubTasksAsync`.
- Risks / constraints: Only one subtask depth is supported.
- Technical context: Subtask operations use task orchestration patterns.

INVEST Check:
- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Pass
- T: Pass

## Open Questions
- None.
