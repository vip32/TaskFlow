---
id: PRD-0107
title: Task Completion and Uncompletion with Subtasks
slice: TASKS
status: Implemented
---

# Product Requirements: Task Completion and Uncompletion with Subtasks

## Overview
Defines completion and uncompletion behavior for tasks, including parent-subtask completion semantics.

## Scope
- In scope: Toggle complete/uncomplete for tasks and subtasks, completion timestamp behavior, parent-to-subtask completion cascade.
- Out of scope: Focus/important/today flags, delete/duplicate/move actions.

## Diagram
```text
Complete(parent)
   |
   +--> parent: IsCompleted=true, CompletedAt=now
   +--> each subtask: Complete()

Uncomplete(parent)
   |
   +--> parent: IsCompleted=false, CompletedAt=null
   +--> subtasks: unchanged

Toggle(subtask)
   |
   +--> only selected subtask completion state changes
```
Covers AC: 1, 2, 3, 4, 5

## User Roles
- Primary: TaskFlow subscription user

## Stories
### Story 1: Set completion state with subtask-aware behavior
- Status: Implemented
- Ready: Yes
- Ready Reason: DoR checks satisfied with explicit parent/subtask completion outcomes.
- User Story: As a user, I want to mark tasks complete or incomplete, so that progress reflects real work state across parent tasks and subtasks.

Acceptance Criteria:
1. When I mark a parent task complete, then the parent task is completed and all attached subtasks are completed.
2. When I uncomplete a parent task, then only the parent task is set to incomplete.
3. When I mark a subtask complete or incomplete, then only that subtask's completion state changes.
4. When a task is completed, then `CompletedAt` is set; when task is uncompleted, `CompletedAt` is cleared.
5. Given a task is already in the requested completion state, when completion is set again, then no duplicate state mutation occurs.

Data Requirements:
- Completion state: `IsCompleted` (bool), `CompletedAt` (DateTime?).
- Hierarchy linkage: `ParentTaskId` for subtask relationships.

Notes:
- Dependencies / external input: `TaskCard.razor`, `TaskListView.razor`, `TaskBoardView.razor`, `TaskEditDialog.razor`, `Home.razor`, `ITaskOrchestrator.SetCompletedAsync`, `Task.Complete`, `Task.Uncomplete`.
- Risks / constraints: Completion cascade is parent-to-subtasks only; uncomplete does not cascade to subtasks.
- Technical context: Completion changes are persisted via task orchestrator and domain task behavior.

INVEST Check:
- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Pass
- T: Pass

## Open Questions
- None.
