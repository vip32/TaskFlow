---
id: PRD-0500
title: Always Show Completed Preference
slice: SUBSCRIPTION
status: Implemented
---

# Product Requirements: Always Show Completed Preference

## Overview
Defines settings behavior for global completed-task visibility.

## Scope
- In scope: `/settings` load/save for completed visibility preference and home behavior impact.
- Out of scope: Additional future settings.

## User Roles
- Primary: TaskFlow subscription user

## Stories
### Story 1: Persist global completed visibility
- Status: Implemented
- Ready: Yes
- Ready Reason: DoR checks satisfied with persistence and rollback behavior.
- User Story: As a user, I want to set global completed visibility, so that list and board behavior is consistent.

Acceptance Criteria:
1. When settings page loads, switch reflects persisted value.
2. When switch changes, value is persisted through settings orchestrator.
3. When global value is true, completed tasks remain visible in home views.
4. When save fails, value rolls back and error feedback is shown.

Data Requirements:
- `SubscriptionSettings.AlwaysShowCompletedTasks: bool`.

Notes:
- Dependencies / external input: `Settings.razor`, `SubscriptionSettingsOrchestrator`, `Home.razor`.
- Risks / constraints: Single exposed preference currently.
- Technical context: `UpdateAlwaysShowCompletedTasksAsync`.

INVEST Check:
- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Pass
- T: Pass

## Open Questions
- None.
