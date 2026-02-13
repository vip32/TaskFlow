---
id: PRD-0106
title: Due Date and Reminders UI
slice: TASKS
status: Pending
---

# Product Requirements: Due Date and Reminders UI

## Overview
Defines pending user-facing workflows for due date and reminder management.

## Scope
- In scope: Task UI for due date/time and reminder configuration.
- Out of scope: External reminder channels.

## User Roles
- Primary: TaskFlow subscription user

## Stories
### Story 1: Schedule tasks with reminders
- Status: Pending
- Ready: No
- Ready Reason: User-facing UI and validation rules are not finalized.
- User Story: As a user, I want to set due dates and reminders, so that time-sensitive tasks are surfaced proactively.

Acceptance Criteria:
1. When I set due date/time in task UI, scheduling fields are persisted.
2. When I add multiple reminders, each reminder is validated and persisted.
3. When reminder input is invalid, save is blocked with actionable message.

Data Requirements:
- Task: `DueDateLocal`, `DueTimeLocal`; reminder collection linked to task.

Notes:
- Dependencies / external input: Pending UI surfaces.
- Risks / constraints: Timezone and DST handling clarity.
- Technical context: Domain/application support exists partially; UI flow missing.

INVEST Check:
- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Fail - Needs split into input/list/edit subflows.
- T: Pass

## Open Questions
- Final reminder validation rules and interaction model.










































