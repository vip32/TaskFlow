---
id: PRD-0800
title: About Page and Version Surface
slice: ABOUT
status: Implemented
---

# Product Requirements: About Page and Version Surface

## Overview
Defines about route content and application version display logic.

## Scope
- In scope: `/about` information card and version fallback behavior.
- Out of scope: Changelog and release-note rendering.

## User Roles
- Primary: TaskFlow subscription user

## Stories
### Story 1: View product summary and running version
- Status: Implemented
- Ready: Yes
- Ready Reason: DoR checks satisfied with explicit fallback chain.
- User Story: As a user, I want to see product info and version, so that I understand what build I am using.

Acceptance Criteria:
1. When I open `/about`, product summary content is visible.
2. When informational version exists, displayed version uses that value without metadata suffix.
3. When informational version is unavailable, fallback uses assembly version or `unknown`.

Data Requirements:
- Version sources: `AssemblyInformationalVersionAttribute`, assembly version fallback.

Notes:
- Dependencies / external input: `About.razor`.
- Risks / constraints: Build metadata availability varies by build mode.
- Technical context: `GetAppVersion` fallback chain.

INVEST Check:
- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Pass
- T: Pass

## Open Questions
- None.










































