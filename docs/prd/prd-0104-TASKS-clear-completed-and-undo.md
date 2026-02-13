---
id: PRD-0104
title: Clear Completed and Undo
slice: TASKS
status: Implemented
---

# Product Requirements: Clear Completed and Undo

## Overview
Defines bulk clear-completed behavior and undo recovery semantics.

## Scope
- In scope: Clear Completed action and undo invocation.
- Out of scope: Detailed single-task delete behavior.

## Diagram
```text
User clicks CLEAR COMPLETED
   -> system removes completed tasks in active context
   -> snackbar shows Undo action
      -> if Ctrl/Cmd+Z or Undo clicked: restore previous task set
      -> else: clear operation remains committed
```
Covers AC: 1, 2, 3, 4

## User Roles
- Primary: TaskFlow subscription user

## Stories
### Story 1: Clear completed tasks safely
- Status: Implemented
- Ready: Yes
- Ready Reason: DoR checks satisfied with destructive and recovery criteria.
- User Story: As a user, I want to clear completed tasks and undo mistakes, so that I can keep lists clean safely.

Acceptance Criteria:
1. When Clear Completed is triggered, completed tasks in current context are removed.
2. When clear action completes, undo path is available.
3. When `Ctrl/Cmd + Z` is invoked, most recent undoable action is restored.
4. When there is no undoable action, state remains stable.

Data Requirements:
- Undo history entries include action payload snapshots.

Notes:
- Dependencies / external input: `IUndoManager`, `Home.razor` key handling.
- Risks / constraints: Undo stack ordering influences restore result.
- Technical context: Undo action registration for destructive operations.

INVEST Check:
- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Pass
- T: Pass

## Open Questions
- None.









































