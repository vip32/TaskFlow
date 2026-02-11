# TaskFlow Design System (MudBlazor)

This document is the UI implementation contract for `TaskFlow.Presentation`.
Use it when building or reviewing Blazor components with MudBlazor.

## Purpose

- Keep UI behavior and styling consistent across screens.
- Define standard MudBlazor usage patterns.
- Reduce regressions in accessibility and responsive behavior.
- Provide a shared PR checklist for frontend changes.

## Scope

Applies to:

- `src/TaskFlow.Presentation/Components/**/*.razor`
- `src/TaskFlow.Presentation/wwwroot/app.css`
- `src/TaskFlow.Presentation/TaskFlowTheme.cs`

Companion docs:

- `docs/VISUAL_UI_UX_GUIDE.md` (visual direction and brand references)
- `docs/REQUIREMENTS.md` (product behavior requirements)

## Source Of Truth

Theme and layout wiring:

- Theme tokens: `src/TaskFlow.Presentation/TaskFlowTheme.cs`
- Theme usage: `src/TaskFlow.Presentation/Components/Layout/MainLayout.razor`
- Global style utilities: `src/TaskFlow.Presentation/wwwroot/app.css`

If this document and implementation diverge, update implementation first, then this doc in the same PR.

## Design Principles

1. Prefer MudBlazor semantic APIs over custom markup/CSS.
2. Keep component responsibilities focused (UI in Presentation, business rules in Domain/Application).
3. Make async state explicit: loading, empty, error.
4. Optimize for keyboard and screen-reader usage by default.
5. Reuse existing TaskFlow patterns before introducing new variants.

## Theme Tokens

Current theme values (from `TaskFlowTheme.cs`):

### Light Palette

- Primary: `#1F86FF`
- Secondary: `#8A2BE2`
- Info: `#2D9CFF`
- Success: `#30C76A`
- Warning: `#F2B01E`
- Error: `#EF4B6C`
- Background: `#F7F8FA`
- Surface: `#FFFFFF`
- AppbarBackground: `#FFFFFF`
- DrawerBackground: `#F7F8FA`
- TextPrimary: `#151821`
- TextSecondary: `#3A3F49`

### Dark Palette

- Primary: `#2D9CFF`
- Secondary: `#D34FAF`
- Info: `#2D9CFF`
- Success: `#30C76A`
- Warning: `#F2B01E`
- Error: `#EF4B6C`
- Background: `#111317`
- Surface: `#1B1E24`
- AppbarBackground: `#1B1E24`
- DrawerBackground: `#1B1E24`
- DrawerText: `#E9ECF1`
- TextPrimary: `#E9ECF1`
- TextSecondary: `#9FA6B2`

### Layout Token

- Default border radius: `12px`

## Semantic Color Rules

Use Mud semantic colors in components:

- Primary action: `Color.Primary`
- Success state: `Color.Success`
- Warning state: `Color.Warning`
- Error/destructive state: `Color.Error`
- Informational state: `Color.Info`
- Neutral/secondary action: `Color.Default`

Rules:

- Do not add new hardcoded hex values for semantic status UI.
- Hex values are allowed for user-defined data (for example, project color selection/swatch).
- For priority visuals, use CSS classes mapped to Mud palette vars:
- `.tf-task-meta-dot-high`
- `.tf-task-meta-dot-medium`
- `.tf-task-meta-dot-low`

## Typography Rules

Use `MudText` with `Typo.*` consistently:

- Page heading: `Typo.h5`
- Section heading: `Typo.h6` or `Typo.subtitle1`
- Body: `Typo.body1`
- Secondary/supporting: `Typo.body2` or `Typo.caption`

Prefer `MudText` over raw paragraph tags for component body copy.

## Spacing And Layout Rules

Primary spacing mechanisms:

1. Mud utility classes (`pa-*`, `ma-*`, `mt-*`, `mb-*`)
2. Shared `tf-` classes in `app.css`

Existing layout class system to reuse:

- `.tf-toolbar`
- `.tf-filters`
- `.tf-create-row`
- `.tf-list`
- `.tf-board`
- `.tf-empty`
- `.tf-shortcuts-backdrop`
- `.tf-shortcuts-dialog`

Keep spacing close to an 8px rhythm.

## Component Patterns

### App Shell

Use:

- `MudLayout`
- `MudAppBar`
- `MudDrawer`
- `MudMainContent`

Reference:

- `src/TaskFlow.Presentation/Components/Layout/MainLayout.razor`

### Navigation Drawer

Use the shared drawer component and app state:

- `src/TaskFlow.Presentation/Components/Layout/HomeNavigationDrawer.razor`

### Task List/Card

Primary list composition:

- `HomeListView` + `TaskCard`

References:

- `src/TaskFlow.Presentation/Components/Pages/HomeListView.razor`
- `src/TaskFlow.Presentation/Components/Pages/TaskCard.razor`

### Board View

Board composition:

- `MudDropContainer`
- `MudDropZone`

Reference:

- `src/TaskFlow.Presentation/Components/Pages/TaskBoardView.razor`

### Dialogs

Preferred pattern for new modals:

- `MudDialog` with `MudDialogProvider`

Current state in repo includes both MudDialog and custom overlay dialog patterns.
Rule: use MudDialog for all new dialogs; migrate legacy overlays opportunistically when touching those flows.

## Required UI States

Any async page/section should model relevant states:

### Loading

```razor
<MudProgressLinear Indeterminate="true" Color="Color.Primary" Class="mb-3" />
```

### Empty

```razor
<MudPaper Elevation="0" Class="tf-empty">
    <MudText Typo="Typo.h6">No tasks yet</MudText>
    <MudText Typo="Typo.body2">Create your first task and keep momentum going.</MudText>
</MudPaper>
```

### Error

```razor
<MudAlert Severity="Severity.Error" Dense="true">
    Something went wrong. Please try again.
</MudAlert>
```

Guideline:

- Snackbar: short action feedback.
- Alert: persistent load/operation errors requiring attention.

## Accessibility Baseline

### Icon Buttons

All icon-only controls must set accessible text:

- `AriaLabel` required
- `Title` recommended

Example:

```razor
<MudIconButton Icon="@Icons.Material.Filled.Delete"
               Color="Color.Error"
               AriaLabel="Delete task"
               Title="Delete task"
               OnClick="OnDeleteAsync" />
```

### Keyboard

Support at minimum:

- Enter/NumpadEnter for create/confirm actions where applicable
- Escape for dismiss/close behavior on dialogs
- Documented shortcuts discoverable in UI

### Color Semantics

Do not rely only on color. Pair state with icon/text where practical.

## Styling Conventions

Order of preference:

1. MudBlazor component parameters
2. Existing `tf-` classes in `app.css`
3. New `tf-` classes in `app.css`
4. Inline style only for dynamic runtime values (example: user-selected project swatch)

Do not create deep CSS overrides against internal MudBlazor selectors unless unavoidable.

## PR Checklist

- Uses Mud semantic colors for actions/state
- No new semantic hardcoded hex in component logic
- Loading/empty/error states are covered
- Icon-only actions include `AriaLabel`
- Responsive behavior validated on mobile and desktop breakpoints
- Existing shared components/patterns reused
- No business logic leaked into Presentation

## Definition Of Done For UI Changes

A UI task is done only when:

1. Component follows the patterns in this document.
2. State handling includes loading and failure paths where relevant.
3. Keyboard and accessibility basics are implemented.
4. Visual behavior is verified in both light/dark mode.
5. PR checklist items are satisfied.

## Known Improvement Targets

These are existing technical debt areas to improve incrementally:

- Replace legacy semantic hex usage in component code with semantic mappings.
- Standardize remaining custom overlay dialogs to MudDialog workflows.
- Ensure icon-only controls consistently include `AriaLabel`.

## Change Management

When adding/changing UI patterns:

1. Update implementation.
2. Update this document in the same PR if a rule/pattern changed.
3. Keep examples minimal and aligned with actual code conventions.
