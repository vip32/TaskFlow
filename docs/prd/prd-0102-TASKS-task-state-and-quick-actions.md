---
id: PRD-0102
title: Task State and Quick Actions
slice: TASKS
status: Implemented
---

# Product Requirements: Task State and Quick Actions

## Overview
Defines toggle and quick-action behavior for active task execution.

## Scope
- In scope: Focus/important/today toggles, duplicate, move, delete.
- Completion/uncompletion semantics are specified in `PRD-0107`.
- Out of scope: Filtering/sorting rules.

## User Roles
- Primary: TaskFlow subscription user

## Stories
### Story 1: Apply one-click task actions
- Status: Implemented
- Ready: Yes
- Ready Reason: DoR checks satisfied with explicit action outcomes.
- User Story: As a user, I want one-click task actions, so that I can keep momentum while managing task state.

Acceptance Criteria:
1. When I toggle focus/important/today, corresponding task state is persisted.
2. When I duplicate a task, then a new task is created from source task context.
3. When I move task to another project, then project assignment is updated.
4. When I delete task, then it is removed and can participate in undo flow.

Data Requirements:
- Task flags: `IsCompleted`, `IsFocused`, `IsImportant`, `IsMarkedForToday`.

Notes:
- Dependencies / external input: `TaskCard.razor`, `Home.razor`, `TaskOrchestrator`.
- Risks / constraints: Completed tasks disable selected actions.
- Technical context: Event callbacks to orchestrator mutations.

INVEST Check:
- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Pass
- T: Pass

## Open Questions
- None.










































