---
id: PRD-0600
title: Keyboard Shortcuts and Help Dialog
slice: UX
status: Implemented
---

# Product Requirements: Keyboard Shortcuts and Help Dialog

## Overview
Defines keyboard shortcuts and discoverability/help dialog behavior.

## Scope
- In scope: Home key handling and shortcuts dialog content/accessibility.
- Out of scope: User-customizable shortcuts.

## User Roles
- Primary: TaskFlow subscription user

## Stories
### Story 1: Use keyboard shortcuts for common actions
- Status: Implemented
- Ready: Yes
- Ready Reason: DoR checks satisfied with explicit key-action mapping.
- User Story: As a user, I want keyboard shortcuts and help, so that I can operate faster.

Acceptance Criteria:
1. When I press `Ctrl/Cmd + /`, shortcuts dialog opens.
2. When Escape is pressed in shortcuts dialog, it closes.
3. When supported shortcut is pressed with valid preconditions, matching action executes.
4. When preconditions are not met, action is not applied.

Data Requirements:
- Shortcut set: `Ctrl/Cmd+Enter`, `Delete`, `Ctrl/Cmd+N`, `Ctrl/Cmd+F`, `Ctrl/Cmd+P`, `Ctrl/Cmd+Z`, `Ctrl/Cmd+/`.

Notes:
- Dependencies / external input: `Home.razor` key handler, `ShortcutsDialog.razor`.
- Risks / constraints: Browser/OS key conflicts.
- Technical context: `HandleShellKeyDown` and dialog open callback.

INVEST Check:
- I: Pass
- N: Pass
- V: Pass
- E: Pass
- S: Pass
- T: Pass

## Open Questions
- None.










































