---
id: PRD-0400
title: Project Lifecycle and Selection
slice: PROJECTS
status: Implemented
---

# Product Requirements: Project Lifecycle and Selection

## Overview
Defines project create/edit/delete/select behavior from the navigation drawer.

## Scope
- In scope: Project dialogs, sidebar selection, project task count badges.
- Out of scope: Per-project view preference behavior.

## User Roles
- Primary: TaskFlow subscription user

## Stories
### Story 1: Manage project records from navigation
- Status: Implemented
- Ready: Yes
- Ready Reason: DoR checks satisfied with dialog and state updates.
- User Story: As a user, I want to create, edit, delete, and select projects, so that I can organize work by context.

Acceptance Criteria:
1. When I create a project, then it is persisted, shown in sidebar, and selected.
2. When I edit a project, then updated values appear in sidebar state.
3. When I delete a project, then it is removed and a valid selection remains.
4. When projects are listed, then each row shows incomplete task count badge.

Data Requirements:
- Project fields: `Name`, `Color`, `Icon`, `ViewType`.

Notes:
- Dependencies / external input: `HomeNavigationDrawer.razor`, `ProjectOrchestrator`, `TaskOrchestrator`.
- Risks / constraints: Active project deletion requires safe reselection.
- Technical context: `AppState.SetNavigationData`, project dialogs.

INVEST Check:
- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Pass
- T: Pass

## Open Questions
- None.










































