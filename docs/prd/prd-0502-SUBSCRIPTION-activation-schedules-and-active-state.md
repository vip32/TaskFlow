---
id: PRD-0502
title: Activation Schedules and Active State
slice: SUBSCRIPTION
status: Partial
---

# Product Requirements: Activation Schedules and Active State

## Overview
Defines subscription schedule windows and how enabled state combines with schedule activity.

## Scope
- In scope: open-ended and bounded schedule windows, active-state evaluation, schedule date validation.
- Out of scope: end-user schedule editing UI and policy automation.

## Diagram
```text
Schedule types
  - Open-ended: startsOn .. infinity
  - Window:     startsOn .. endsOn (inclusive)

IsActiveAt(date):
  if IsEnabled == false -> false
  else if any schedule active at date -> true
  else -> false
```
Covers AC: 1, 2, 3, 4

## User Roles
- Primary: TaskFlow subscription user
- Secondary: operator/configuration owner

## Stories
### Story 1: Evaluate subscription activity from schedule windows
- Status: Partial
- Ready: Yes
- Ready Reason: Domain rules and runtime schedule seeding are implemented; direct user workflow is not exposed.
- User Story: As an operator, I want subscription activity to follow schedule windows and enablement state, so that access behavior can be controlled by date.

Acceptance Criteria:
1. When an open-ended schedule is added, it is active on and after `StartsOn`.
2. When a bounded schedule is added, it is active from `StartsOn` through `EndsOn` inclusive.
3. Given a schedule where `EndsOn` is before `StartsOn`, creation fails with validation error.
4. When `IsEnabled` is false, `IsActiveAt` returns false even if a schedule would be active.
5. When `IsEnabled` is true and any schedule is active for the date, `IsActiveAt` returns true.

Data Requirements:
- `SubscriptionSchedule.StartsOn: DateOnly`
- `SubscriptionSchedule.EndsOn: DateOnly?`
- `Subscription.IsEnabled: bool`

Notes:
- Dependencies / external input: `Subscription.cs`, `SubscriptionSchedule.cs`, `CurrentSubscriptionAccessor.cs`.
- Risks / constraints: No schedule management surface exists in current UI.
- Technical context: Runtime accessor seeds schedule from configuration (`Subscription:Schedule:StartsOn`, `Subscription:Schedule:EndsOn`).

INVEST Check:
- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Pass
- T: Pass

## Open Questions
- Should multiple schedule windows be editable via UI, and what conflict rules should apply?
