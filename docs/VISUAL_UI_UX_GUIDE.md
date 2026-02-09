# TaskFlow Visual UI/UX Guide

This guide defines the visual language reflected in `docs/visuals`.
It is the baseline for a responsive web app experience across mobile and desktop browsers.

## Source Visuals

- Desktop reference set: `docs/visuals/Screenshot-mac-*.png` (2880x1800)
- Mobile reference set: `docs/visuals/Screenshot-phone-*.png` (1242x2688)

## Visual Direction

- Core theme: focused productivity with premium, modern web polish.
- Mood split:
  - Light mode: clean, airy, high legibility.
  - Dark mode: deep contrast with warm accent highlights.
- Brand framing style: bold headline over soft layered gradients, with device mockups as the focal point.

## Color System

Use these as canonical tokens for visual production.

### Core Brand Colors

- `Brand/Purple-700`: `#5B1ECF`
- `Brand/Purple-500`: `#8A2BE2`
- `Brand/Pink-500`: `#D34FAF`
- `Brand/Blue-500`: `#1F86FF`
- `Brand/Blue-300`: `#A9D9FF`

### UI Accent Colors

- `Accent/Info`: `#2D9CFF`
- `Accent/Success`: `#30C76A`
- `Accent/Warning`: `#F2B01E`
- `Accent/Danger`: `#EF4B6C`

### Neutral Colors (Light)

- `Neutral/0`: `#FFFFFF`
- `Neutral/50`: `#F7F8FA`
- `Neutral/100`: `#ECEEF2`
- `Neutral/400`: `#9AA1AE`
- `Neutral/700`: `#3A3F49`
- `Neutral/900`: `#151821`

### Neutral Colors (Dark)

- `Dark/Background`: `#111317`
- `Dark/Surface`: `#1B1E24`
- `Dark/Panel`: `#2A2E36`
- `Dark/Border`: `#3B404A`
- `Dark/TextPrimary`: `#E9ECF1`
- `Dark/TextSecondary`: `#9FA6B2`

## Gradients and Backdrops

Use soft radial/diagonal overlays to create depth behind device mockups.

- Hero gradient A (Desktop light): `#5B1ECF -> #D34FAF`
- Hero gradient B (Mobile light): `#A9D9FF -> #1F86FF`
- Hero gradient C (Dark mode): `#4F1BAA -> #1A0F3E`

Guidelines:

- Keep background contrast lower than headline and device frame.
- Use 2-3 overlapping gradient shapes only.
- Avoid noisy textures; visuals should remain calm and premium.

## Typography

Visual typography pattern in assets:

- Headline style: extra-bold sans-serif, centered or top-left.
- Supporting copy: medium/semibold sans-serif.
- UI text: regular/medium sans-serif, compact but readable.

Recommended font stack:

- `font-display`: `SF Pro Display`, `Inter`, `Segoe UI`, sans-serif
- `font-text`: `SF Pro Text`, `Inter`, `Segoe UI`, sans-serif

Type scale:

- `Display XL`: 88-112px, 700-800 weight
- `Display L`: 56-72px, 700-800 weight
- `Heading`: 36-48px, 700 weight
- `Body`: 16-20px, 400-500 weight
- `UI Small`: 12-14px, 500 weight

## Layout Principles

- Prioritize one primary device focus per frame.
- Keep large safe margins around headlines.
- Use diagonal composition when showing multiple devices.
- Keep screenshot crop intentional: show meaningful task workflow state.

### Desktop Web Composition

- Canvas ratio: 16:10.
- Top 30-40% reserved for messaging.
- Product UI placed center/lower-third.
- Surface shadows should be soft, not dramatic.

### Mobile Web Composition

- Canvas ratio: approx 9:19.5 portrait.
- Headline above primary viewport crop, often left-aligned for readability.
- Main UI panel can bleed off canvas edge for dynamism.
- Keep browser-safe top/bottom spacing for status/address bar variance.

## Responsive Rules

- Build mobile-first, then scale up to desktop.
- Recommended breakpoints:
  - `sm`: 640px
  - `md`: 768px
  - `lg`: 1024px
  - `xl`: 1280px
- Navigation behavior:
  - Mobile: bottom nav or compact top nav with quick actions.
  - Desktop: persistent left sidebar plus content/detail panes.
- Layout behavior:
  - Mobile: single-column priority flow.
  - Tablet: optional 2-pane view.
  - Desktop: 2-3 pane productivity layout.

## Blazor + MudBlazor 8.x Implementation

Use this guide through MudBlazor theming and component conventions, not ad-hoc CSS-first styling.

### Theme Setup

- Define colors in a central `MudTheme` (`PaletteLight` and `PaletteDark`).
- Map guide tokens to MudBlazor semantic slots:
  - Primary: `Brand/Blue-500` or `Brand/Purple-500` (choose one global primary).
  - Secondary: complementary brand hue.
  - Success/Warning/Error/Info: map directly from `Accent/*`.
  - Background/Surface/Appbar/Drawer: map from light/dark neutral sets.
- Keep one source of truth for spacing, radii, and elevation in shared style tokens.

### Component Mapping

- Navigation shell:
  - `MudLayout` + `MudAppBar` + `MudDrawer` for responsive app chrome.
  - Mobile: temporary drawer or bottom action zone.
  - Desktop: persistent left drawer.
- Task lists and cards:
  - `MudPaper` or `MudCard` for grouped surfaces.
  - `MudList` + `MudListItem` for compact task rows.
  - `MudChip` for project/status pills and metadata tags.
- Forms and task detail:
  - `MudForm`, `MudTextField`, `MudSelect`, `MudDatePicker`, `MudSwitch`.
  - Keep validation messaging inline and concise.
- Actions:
  - `MudButton` for primary/secondary actions.
  - `MudIconButton` only when icon meaning is obvious; add `AriaLabel`.

### Responsive Behavior in Razor Components

- Use MudBlazor breakpoint services/utilities to switch layout behavior at `sm/md/lg/xl`.
- Prefer conditional rendering for major layout shifts (single pane vs multi-pane).
- Keep tap targets and row density comfortable on mobile before optimizing desktop density.

### Styling Rules for MudBlazor

- Prefer theme overrides and component parameters before custom CSS.
- Use CSS only for brand-specific visual identity layers (hero gradients, marketing blocks).
- Avoid deep selector overrides against internal MudBlazor markup unless unavoidable.
- Keep custom class names semantic and feature-scoped.

### Motion and Interaction

- Use subtle transitions (`150-250ms`) on hover, focus, and panel open/close states.
- Respect `prefers-reduced-motion` for non-essential animation.
- Ensure loading/disabled states are explicit on submit and async actions.

## Component Styling Patterns

Observed recurring UI styling in screenshots:

- Sidebar-first information architecture.
- Rounded card surfaces for tasks and panels.
- Low-chroma borders and separators.
- Color-coded task/project markers.
- Clear iconography with small numeric counters.

### Corners

- Main app surfaces: 10-14px radius
- Cards/task rows: 8-12px radius
- Pills/chips: full radius (999px)

### Shadows

- Light mode: subtle outer shadow, low blur contrast
- Dark mode: mostly border/tonal separation instead of heavy glow

### Spacing

- UI base unit: 8px
- Dense rows: 8-12px vertical
- Standard panel padding: 16-24px

## Iconography and Indicators

- Use simple, rounded, system-like icons.
- Keep icon stroke/shape consistent across nav and lists.
- Encode status semantically:
  - Today: yellow
  - Upcoming: purple
  - Focus: green
  - Project labels: blue/cyan family

## Imagery Rules

- Always use full-resolution source screenshots.
- Prefer real in-app states over abstract placeholders.
- Ensure task names and metadata look realistic and narrative.
- Avoid mixing unrelated visual themes in one frame.

## Dark Mode Rules

- Maintain high text contrast; do not reduce readability for style.
- Reserve warm amber/yellow for high-attention actions.
- Use cool blue/cyan for project grouping and metadata tags.
- Keep background layers distinguishable via value contrast, not saturation spikes.

## Accessibility Baseline

- Body text contrast target: WCAG AA (4.5:1 minimum).
- Interactive hit targets in web UI: at least 44x44 px.
- Never rely on color only; pair with icon or label.
- Keep large headline text above 3:1 contrast.

## Do and Don’t

Do:

- Use bold, minimal messaging with one key promise.
- Keep visual hierarchy: headline -> device -> UI detail.
- Reuse the established gradient families and accent logic.

Don’t:

- Add cluttered multi-shadow effects or glossy skeuomorphic chrome.
- Mix too many accent hues in one screen.
- Use tiny, low-contrast labels on dark surfaces.

## Practical Production Checklist

- Pick canvas ratio (`16:10` for desktop, `9:19.5` for mobile).
- Apply one approved gradient family.
- Place headline with clear whitespace.
- Insert authentic screenshot at native aspect ratio.
- Apply subtle shadow and edge separation.
- Validate contrast and export at full resolution.
