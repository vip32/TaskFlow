---
id: PRD-0103
title: Task Filter Search Sort and Visibility
slice: TASKS
status: Implemented
---

# Product Requirements: Task Filter Search Sort and Visibility

## Overview
Defines task query controls for search, filter, sorting, and completed visibility.

## Scope
- In scope: Search by title/note, priority/status filters, sort modes, local completed toggle.
- Out of scope: Global setting persistence workflow.

## User Roles
- Primary: TaskFlow subscription user

## Stories
### Story 1: Refine visible task set
- Status: Implemented
- Ready: Yes
- Ready Reason: DoR checks satisfied with deterministic filter outputs.
- User Story: As a user, I want filtering and sorting controls, so that I can focus on relevant tasks.

Acceptance Criteria:
1. When search text changes, task list/board source updates by title or note match.
2. When priority/status filters are set, only matching tasks remain visible.
3. When sort mode changes, order follows selected mode.
4. When local Show Completed is off and global override is false, completed tasks are hidden.
5. When global AlwaysShowCompletedTasks is true, completed tasks stay visible.

Data Requirements:
- Query state: `searchQuery`, `priorityFilter`, `statusFilter`, `sortMode`, `showCompleted`, `alwaysShowCompletedTasks`.

Notes:
- Dependencies / external input: `Home.razor` query pipeline and settings read.
- Risks / constraints: Combined filters may yield empty view.
- Technical context: Filtered task builder used by list and board.

INVEST Check:
- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Pass
- T: Pass

## Open Questions
- None.










































