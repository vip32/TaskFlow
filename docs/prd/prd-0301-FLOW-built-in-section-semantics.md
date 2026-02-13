---
id: PRD-0301
title: Built In Section Semantics
slice: FLOW
status: Implemented
---

# Product Requirements: Built In Section Semantics

## Overview
Defines built-in section set and section task-resolution semantics.

## Scope
- In scope: `Recent`, `Today`, `Important`, `This Week`, `Upcoming` built-ins and rule resolution.
- Out of scope: Custom section management UI.

## User Roles
- Primary: TaskFlow subscription user

## Stories
### Story 1: Resolve tasks for built-in flow sections
- Status: Implemented
- Ready: Yes
- Ready Reason: DoR checks satisfied for currently surfaced built-ins.
- User Story: As a user, I want built-in sections to show relevant tasks, so that I can plan by urgency and importance.

Acceptance Criteria:
1. When app seeds sections, built-ins include Recent, Today, Important, This Week, Upcoming.
2. When I select a built-in section, tasks are resolved by section matching rules.
3. When date buckets are evaluated, subscription timezone is applied to local boundaries.
4. When tasks do not match section rules, they are excluded.

Data Requirements:
- `TaskFlowDueBucket`, section rule flags, subscription `TimeZoneId`.

Notes:
- Dependencies / external input: `MyTaskFlowSectionOrchestrator.GetSectionTasksAsync`.
- Risks / constraints: Custom section UI is not yet available.
- Technical context: Rule match and ordered result composition.

INVEST Check:
- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Pass
- T: Pass

## Open Questions
- None.










































