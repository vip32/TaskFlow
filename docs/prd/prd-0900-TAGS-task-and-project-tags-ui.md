---
id: PRD-0900
title: Task and Project Tags UI
slice: TAGS
status: Pending
---

# Product Requirements: Task and Project Tags UI

## Overview
Defines pending tag management workflows for tasks and projects.

## Scope
- In scope: Tag create, assign/remove, and filter use.
- Out of scope: Advanced taxonomy governance.

## User Roles
- Primary: TaskFlow subscription user

## Stories
### Story 1: Classify work with tags
- Status: Pending
- Ready: No
- Ready Reason: UI controls and validation constraints are not implemented.
- User Story: As a user, I want tags in UI, so that I can group and find related work quickly.

Acceptance Criteria:
1. When I create a tag, it becomes available for assignment.
2. When I assign/remove tags on tasks/projects, associations persist.
3. When I filter by tags, visible items match selected tags.

Data Requirements:
- Tag entities and task/project associations.

Notes:
- Dependencies / external input: Missing tag UI surfaces.
- Risks / constraints: Tag naming constraints and ownership model.
- Technical context: Requires end-user orchestration and filtering surfaces.

INVEST Check:
- I: Pass
- N: Pass
- V: Pass
- E: Fail - Detailed constraints needed for estimation.
- S: Pass
- T: Pass

## Open Questions
- Tag uniqueness scope and validation rules.










































