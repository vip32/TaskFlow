---
id: PRD-0300
title: Section Selection and Navigation
slice: FLOW
status: Implemented
---

# Product Requirements: Section Selection and Navigation

## Overview
Defines My Task Flow section selection behavior from the navigation drawer.

## Scope
- In scope: Section list rendering, section selection, context switch on `/`.
- Out of scope: Section rule semantics.

## Diagram
```text
Sidebar
  |
  +--> Select section (Recent/Today/Important/This Week/Upcoming)
          -> activeSection = selected
          -> selectedProject = null
          -> navigate "/"
          -> load/render section tasks
```
Covers AC: 1, 2, 3

## User Roles
- Primary: TaskFlow subscription user

## Stories
### Story 1: Navigate between flow sections
- Status: Implemented
- Ready: Yes
- Ready Reason: DoR checks satisfied with deterministic selection state behavior.
- User Story: As a user, I want to switch between flow sections, so that I can process cross-project work buckets.

Acceptance Criteria:
1. When I click a flow section, section becomes active and home route is shown.
2. When section is active, selected project is cleared.
3. When navigation data loads, sections render in configured order.

Data Requirements:
- Section data: `Id`, `Name`, `SortOrder`.

Notes:
- Dependencies / external input: `HomeNavigationDrawer.razor`, `AppState`, `IMyTaskFlowSectionOrchestrator.GetAllAsync`.
- Risks / constraints: Requires section seed data.
- Technical context: `SelectSection` behavior updates state and route.

INVEST Check:
- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Pass
- T: Pass

## Open Questions
- None.










































