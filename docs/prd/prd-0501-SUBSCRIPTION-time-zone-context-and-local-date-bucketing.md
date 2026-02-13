---
id: PRD-0501
title: Time Zone Context and Local Date Bucketing
slice: SUBSCRIPTION
status: Partial
---

# Product Requirements: Time Zone Context and Local Date Bucketing

## Overview
Defines how subscription time zone is sourced and applied to local date-driven task flow behavior.

## Scope
- In scope: subscription `TimeZoneId` validation, runtime subscription context, local date bucketing used by flow/task queries.
- Out of scope: end-user timezone management UI.

## Diagram
```text
Config: Subscription:TimeZoneId
   -> CurrentSubscriptionAccessor
      -> Subscription(TimeZoneId validated)
         -> Task/Flow orchestrators resolve TimeZoneInfo
            -> local date windows (Today/This Week/Upcoming)
```
Covers AC: 1, 2, 3

## User Roles
- Primary: TaskFlow subscription user
- Secondary: operator/configuration owner

## Stories
### Story 1: Apply subscription timezone to local date behavior
- Status: Partial
- Ready: Yes
- Ready Reason: Domain/runtime behavior is implemented; user-facing management workflow is missing.
- User Story: As a subscription user, I want task flow date buckets to use my subscription timezone, so that Today/This Week/Upcoming behave consistently.

Acceptance Criteria:
1. Given a configured subscription timezone, when runtime subscription context is created, then `TimeZoneId` is validated and stored on the subscription.
2. When task/flow orchestrators compute local-date windows, then they resolve `TimeZoneInfo` from the subscription timezone.
3. Given invalid timezone input, when subscription timezone is set, then operation fails with a validation exception.
4. When timezone configuration is missing, then runtime falls back to the default timezone.

Data Requirements:
- `Subscription.TimeZoneId: string` (IANA/system timezone id).

Notes:
- Dependencies / external input: `Subscription.cs`, `CurrentSubscriptionAccessor.cs`, `TaskOrchestrator.cs`, `MyTaskFlowSectionOrchestrator.cs`.
- Risks / constraints: No UI exists to manage timezone today; behavior is config-driven.
- Technical context: Timezone affects local date boundaries in flow/task query orchestration.

INVEST Check:
- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Pass
- T: Pass

## Open Questions
- Should timezone management be exposed in `/settings` for end users?
