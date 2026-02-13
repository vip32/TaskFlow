---
id: PRD-0000
title: Home Navigation and Context Selection
slice: SHELL
status: Implemented
---

# Product Requirements: Home Navigation and Context Selection

## Overview
Defines how the home shell switches between project and My Task Flow section contexts.

## Scope
- In scope: Context selection from drawer and active-context rendering on `/`.
- Out of scope: Project CRUD internals and task action semantics.

## User Roles
- Primary: TaskFlow subscription user

## Stories
### Story 1: Switch active work context
- Status: Implemented
- Ready: Yes
- Ready Reason: DoR checks satisfied with concrete UI state transitions.
- User Story: As a user, I want to switch between project and section contexts, so that I can focus on the right task set.

Acceptance Criteria:
1. When I select a project in the drawer, then `/` shows project context and clears selected section.
2. When I select a section in the drawer, then `/` shows section context and clears selected project.
3. When no explicit selection exists on initial load, then first available project is selected.
4. When selected context no longer exists, then selection falls back to a valid context.

Data Requirements:
- `SelectedProjectId: Guid?` and `SelectedSectionId: Guid?` are mutually exclusive.

Notes:
- Dependencies / external input: `AppState`, `HomeNavigationDrawer.razor`, `Home.razor`.
- Risks / constraints: Single active subscription context.
- Technical context: `AppState.SelectProject`, `AppState.SelectSection`.

INVEST Check:
- I: Pass - Isolated shell context behavior.
- N: Pass - UI labels can change without changing semantics.
- V: Pass - Core navigation behavior.
- E: Pass - Criteria are explicit.
- S: Pass - Focused scope.
- T: Pass - Observable in UI.

## Open Questions
- None.









































