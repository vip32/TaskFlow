---
id: PRD-0200
title: Board Lanes and Drag Drop
slice: BOARD
status: Implemented
---

# Product Requirements: Board Lanes and Drag Drop

## Overview
Defines board lane structure, lane task creation, and drag/drop status transitions.

## Scope
- In scope: Todo/Doing/Done lanes, counts, drop behavior, create-in-lane.
- Out of scope: View mode persistence.

## Diagram
```text
[Todo]  <-->  [Doing]  <-->  [Done]
   ^                        /
   +-----------------------+

Create in lane:
User enters title in lane composer -> task created with lane status

Drop rule:
if source == destination -> no-op
```
Covers AC: 1, 2, 3, 4, 5

## User Roles
- Primary: TaskFlow subscription user

## Stories
### Story 1: Execute work on a kanban board
- Status: Implemented
- Ready: Yes
- Ready Reason: DoR checks satisfied with explicit lane and status rules.
- User Story: As a user, I want draggable board lanes, so that I can manage progress visually.

Acceptance Criteria:
1. When board mode is active, lanes `Todo`, `Doing`, and `Done` are displayed with counts.
2. When I drag a task to another lane, task status updates to that lane.
3. When I add a task title inside lane composer, task is created in lane status.
4. When lane is empty, drop hint is visible.
5. Task completed state is untouched by lane changes.

Data Requirements:
- Board statuses: `Todo`, `Doing`, `Done`.

Notes:
- Dependencies / external input: `TaskBoardView.razor`, Home board handlers.
- Risks / constraints: Drop is ignored when source and destination are equal.
- Technical context: `MudDropContainer` and `OnDropTaskToStatus`.

INVEST Check:
- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Pass
- T: Pass

## Open Questions
- None.
