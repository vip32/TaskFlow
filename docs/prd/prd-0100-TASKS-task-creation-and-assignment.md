---
id: PRD-0100
title: Task Creation and Assignment
slice: TASKS
status: Implemented
---

# Product Requirements: Task Creation and Assignment

## Overview
Defines creating tasks in active project or section context with default priority.

## Scope
- In scope: Create task input/add action, project/unassigned assignment outcome.
- Out of scope: Editing and advanced actions.

## User Roles
- Primary: TaskFlow subscription user

## Stories
### Story 1: Create tasks quickly in current context
- Status: Implemented
- Ready: Yes
- Ready Reason: DoR checks satisfied with clear create outcomes.
- User Story: As a user, I want to create tasks from home, so that I can capture work immediately.

Acceptance Criteria:
1. When I submit a non-empty title, then a task is created.
2. Given selected project context, created task is assigned to that project.
3. Given selected section context, created task can remain unassigned.
4. When no priority is set, default is `Medium`.

Data Requirements:
- Task: `Title`, `Priority`, `ProjectId?`, `Status`.

Notes:
- Dependencies / external input: `Home.razor`, `TaskOrchestrator`.
- Risks / constraints: Empty titles are ignored.
- Technical context: Create flow branches by selected context.

INVEST Check:
- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Pass
- T: Pass

## Open Questions
- None.










































