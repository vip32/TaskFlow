---
id: PRD-0101
title: Task Editing Inline and Dialog
slice: TASKS
status: Implemented
---

# Product Requirements: Task Editing Inline and Dialog

## Overview
Defines inline title edit behavior and edit-dialog access for expanded edits.

## Scope
- In scope: Inline title edit commit/cancel and edit dialog open.
- Out of scope: Quick-action toggles and deletes.

## User Roles
- Primary: TaskFlow subscription user

## Stories
### Story 1: Edit task content from list and board
- Status: Implemented
- Ready: Yes
- Ready Reason: DoR checks satisfied with explicit edit guards.
- User Story: As a user, I want inline and dialog editing, so that I can update tasks at the right level of detail.

Acceptance Criteria:
1. When inline edit submits a changed non-empty title, then title is persisted.
2. When inline edit is cancelled with Escape, then original title remains.
3. When edit icon is clicked, then task dialog opens.
4. When edited title is blank or unchanged, then no update is applied.

Data Requirements:
- Editable field: `Title` inline; additional fields through dialog.

Notes:
- Dependencies / external input: `TaskListView.razor`, `TaskBoardView.razor`, `TaskEditDialog.razor`.
- Risks / constraints: Completed tasks have disabled edit affordances.
- Technical context: `OnUpdateTaskTitleRequested` callback path.

INVEST Check:
- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Pass
- T: Pass

## Open Questions
- None.










































