---
id: PRD-0401
title: Project View Mode Preference
slice: PROJECTS
status: Implemented
---

# Product Requirements: Project View Mode Preference

## Overview
Defines persisted list/board preference per project.

## Scope
- In scope: View toggle action and persistence for selected project.
- Out of scope: Board interaction details.

## User Roles
- Primary: TaskFlow subscription user

## Stories
### Story 1: Persist preferred project view mode
- Status: Implemented
- Ready: Yes
- Ready Reason: DoR checks satisfied with explicit persistence behavior.
- User Story: As a user, I want each project to remember list or board mode, so that I return to my preferred workflow.

Acceptance Criteria:
1. When I toggle view mode, then project switches between `List` and `Board`.
2. When mode changes, then preference is persisted.
3. When I revisit the project, then previously saved mode is restored.

Data Requirements:
- `Project.ViewType` enum: `List | Board`.

Notes:
- Dependencies / external input: `Home.razor`, `ProjectOrchestrator.UpdateViewTypeAsync`.
- Risks / constraints: Applies only when a project is selected.
- Technical context: UI branch on `selectedProject.ViewType`.

INVEST Check:
- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Pass
- T: Pass

## Open Questions
- None.










































