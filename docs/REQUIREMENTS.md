# TaskFlow - Requirements Document

## Table of Contents
1. [Document Overview](#document-overview)
2. [Functional Requirements](#functional-requirements)
3. [Non-Functional Requirements](#non-functional-requirements)
4. [User Stories](#user-stories)
5. [Acceptance Criteria](#acceptance-criteria)
6. [Constraints](#constraints)
7. [Future Enhancements](#future-enhancements)

---

## Document Overview

### Purpose
This document captures all functional and non-functional requirements for TaskFlow, a productivity application built with Blazor Server and prepared for SaaS-style subscription boundaries.

### Scope
TaskFlow provides task and project management with:
- Subscription-scoped data ownership (each user has exactly one subscription that owns projects, tasks, and focus sessions)
- Subscription schedules with active windows (from/to, open-ended end date supported)
- Subscription tiers: Free (limited), Plus (less limited), Pro (full)
- Project organization
- Task prioritization
- SubTask support
- Kanban board views
- My Task Flow smart views (Today, Upcoming, Recent)
- Focus pin for highlighting important tasks
- Advanced UX features (search, sort, keyboard shortcuts)
- Private network access via Tailscale

### Target Audience
- **Primary (current)**: Personal deployment with a single subscription
- **Primary (target)**: Multi-user SaaS model where each user operates within exactly one subscription boundary
- **Environment**: Raspberry Pi 5, Debian 13, private Tailscale network
- **Scale**: Per subscription, 10-50 projects and 100-1000 tasks in baseline usage

### Version
- **Version**: 1.0
- **Date**: February 9, 2026
- **Status**: Approved for Implementation

---

## Functional Requirements

### FR1: Project Management

#### FR1.1 Create Project
**Description**: User must be able to create new projects to organize tasks.

**Requirements**:
- User can specify project name (string, 1-100 characters)
- User can select project color (hex color code)
- User can select project icon (Material Design icon name)
- System generates unique GUID for project ID
- System sets CreatedAt timestamp to current time
- System sets IsDefault to false for all created projects
- System sets default ViewType to List

**Priority**: Must Have

#### FR1.2 View Projects
**Description**: User must be able to view all projects in a sidebar navigation.

**Requirements**:
- System displays project list in sidebar
- Each project item shows: Icon, Name, Task count badge, Color indicator
- Active project is highlighted with Turquoise accent (#40E0D0)
- Projects are sorted by creation date (newest last)
- Sidebar is responsive (collapsible on mobile, 250px on desktop)

**Priority**: Must Have

#### FR1.3 Edit Project
**Description**: User must be able to edit project properties.

**Requirements**:
- User can update project name
- User can change project color
- User can change project icon
- Changes are persisted to database immediately
- UI updates in real-time via SignalR

**Priority**: Must Have

#### FR1.4 Delete Project
**Description**: User must be able to delete projects.

**Requirements**:
- User can delete any project except default "Inbox" project
- System shows confirmation dialog before deletion
- Deleting a project deletes all associated tasks and subtasks
- System shows toast notification with UNDO option
- Undo restores project and all associated tasks
- System automatically selects another project after deletion

**Priority**: Must Have

#### FR1.5 Default Project
**Description**: System must provide a default "Inbox" project.

**Requirements**:
- System creates default project named "Inbox" on first startup
- Default project has IsDefault flag set to true
- Default project cannot be deleted
- Default project uses Turquoise color (#40E0D0)
- Default project uses "inbox" icon
- Default project is pre-seeded with 3 sample todos

**Priority**: Must Have

#### FR1.6 Project Task Count
**Description**: System must display task count for each project.

**Requirements**:
- Task count shows total tasks (excluding completed by default)
- Badge updates in real-time when tasks are added/removed/completed
- Count includes SubTasks

**Priority**: Must Have

---

### FR2: Task Management

#### FR2.1 Create Task
**Description**: User must be able to create new tasks within a project.

**Requirements**:
- User enters task title (string, 1-500 characters)
- Task is automatically assigned to current project
- System generates unique GUID for task ID
- System sets Priority to Medium (2) by default
- System sets IsCompleted to false
- System sets IsFocused to false
- System sets Status to ToDo
- System sets CreatedAt to current timestamp
- System sets ProjectId to current project ID
- Task appears immediately in list via SignalR

**Priority**: Must Have

#### FR2.2 View Tasks
**Description**: User must be able to view tasks in a project.

**Requirements**:
- System displays tasks in List view or Board view
- List view shows: Checkbox, Focus pin, Title, Note preview, Priority badge, Timestamps, Actions
- Board view shows tasks in 3 columns (ToDo, InProgress, Done)
- Tasks are sorted by CreatedAt (newest first) by default, with focused tasks at top
- User can toggle sorting by Priority or CreatedAt or Focused
- User can filter by priority (All, High, Medium, Low)
- User can show/hide completed tasks
- Empty state displayed when no tasks exist

**Priority**: Must Have

#### FR2.3 Edit Task
**Description**: User must be able to edit task properties.

**Requirements**:
- In-line editing for task title (click to edit)
- Optional note field (expandable text area)
- Dropdown to change priority (High, Medium, Low)
- Priority changes update badge color immediately
- Changes are saved on blur or Enter key
- Changes persist to database immediately
- UI updates in real-time via SignalR
- No explicit Save button is used for task edits

**Priority**: Must Have

#### FR2.4 Delete Task
**Description**: User must be able to delete tasks.

**Requirements**:
- User can delete any task
- System shows toast notification with UNDO option
- Deleting a parent task deletes all SubTasks
- Undo restores task and all SubTasks
- System automatically focuses on next/previous task

**Priority**: Must Have

#### FR2.5 Complete Task
**Description**: User must be able to mark tasks as complete.

**Requirements**:
- User clicks checkbox to toggle completion
- CompletedAt timestamp set to current time
- Task visually marked as completed (strikethrough, faded)
- System shows success toast notification
- If task has SubTasks, all SubTasks are also marked complete
- Completed tasks can be uncompleted (restores to previous state)

**Priority**: Must Have

#### FR2.6 Task Priority
**Description**: Tasks must have priority levels with visual indicators.

**Requirements**:
- Three priority levels: Low (1), Medium (2), High (3)
- Color coding: High=Red (#FF6B6B), Medium=Orange (#F57C00), Low=Green (#10B981)
- Priority badge displays on task items
- Filter by priority in toolbar

**Priority**: Must Have

#### FR2.7 Task Note
**Description**: Tasks can have optional note text for additional information.

**Requirements**:
- Note field is optional (nullable)
- Note can be multi-line text
- Note displays expandable under task title
- In-line editing for note content
- Note persists to database
- Changes save on blur or Ctrl/Cmd + Enter

**Priority**: Must Have

#### FR2.8 Task Focus Pin
**Description**: Tasks can be marked with a focus pin to show/sort differently.

**Requirements**:
- Focus pin toggles IsFocused boolean
- Focused tasks appear at top of lists
- Focused tasks have visual indicator (pin icon)
- Multiple tasks can be focused simultaneously
- Focus state persists across sessions
- Sort option "Focused" shows only focused tasks

**Priority**: Must Have

#### FR2.9 Task Timestamps
**Description**: System must track task creation and completion times.

**Requirements**:
- CreatedAt timestamp set on creation
- CompletedAt timestamp set on completion
- Timestamps display in human-readable format (e.g., "2 hours ago")
- Full datetime available on hover

**Priority**: Must Have

---

### FR3: SubTask Management

#### FR3.1 Create SubTask
**Description**: User must be able to create SubTasks under a parent task.

**Requirements**:
- User can add SubTask to any task
- SubTask is a Task entity with ParentTaskId set
- SubTask inherits project from parent
- SubTasks are 1-level only (no nested sub-subtasks)
- Add SubTask input appears under parent task
- SubTask count badge shows on parent task

**Priority**: Must Have (Phase 2)

#### FR3.2 View SubTasks
**Description**: User must be able to view SubTasks under parent tasks.

**Requirements**:
- SubTasks display indented under parent
- Visual hierarchy maintained
- SubTasks can be expanded/collapsed
- Collapse state persists per session
- SubTasks shown in both List and Board views

**Priority**: Must Have (Phase 2)

#### FR3.3 Edit SubTask
**Description**: User must be able to edit SubTask properties.

**Requirements**:
- In-line editing for SubTask title
- SubTask priority can be changed independently
- Changes update immediately

**Priority**: Must Have (Phase 2)

#### FR3.4 Delete SubTask
**Description**: User must be able to delete SubTasks.

**Requirements**:
- User can delete individual SubTasks
- Toast notification with UNDO option
- Deleting SubTask updates parent SubTask count

**Priority**: Must Have (Phase 2)

#### FR3.5 Auto-Complete SubTasks
**Description**: Completing parent task must complete all SubTasks.

**Requirements**:
- Marking parent task complete marks all SubTasks complete
- Uncompleting parent task does NOT uncomplete SubTasks
- Visual indicator when parent has completed SubTasks

**Priority**: Must Have (Phase 2)

---

### FR4: Board View (Kanban)

#### FR4.1 Board Layout
**Description**: User must be able to view tasks in Kanban board layout.

**Requirements**:
- 3 columns: ToDo, InProgress, Done
- Each column shows tasks with that Status
- Task count badge per column
- Drag-and-drop to move tasks between columns
- Add task button on each column
- Columns stack on mobile (<768px)

**Priority**: Must Have (Phase 2)

#### FR4.2 Switch Views
**Description**: User must be able to switch between List and Board views.

**Requirements**:
- View switcher toggle in toolbar
- View type persists per project
- Switching views preserves current filters and sort
- State maintained via SignalR

**Priority**: Must Have (Phase 2)

#### FR4.3 Move Tasks
**Description**: User must be able to move tasks between statuses.

**Requirements**:
- Drag-and-drop between columns
- Status updates immediately on drop
- Visual feedback during drag
- Tasks with subtasks show indicator

**Priority**: Must Have (Phase 2)

#### FR4.4 Add to Column
**Description**: User must be able to add tasks directly to board columns.

**Requirements**:
- Each column has add task input
- New task gets column's Status
- Task appears immediately in column

**Priority**: Must Have (Phase 2)

---

### FR5: Search, Sort, Filter

#### FR5.1 Search Tasks
**Description**: User must be able to search tasks by title.

**Requirements**:
- Real-time search as user types
- Case-insensitive search
- Searches across all tasks in current project
- Highlights matching text
- Clears search on Escape key

**Priority**: Must Have

#### FR5.2 Sort Tasks
**Description**: User must be able to sort tasks.

**Requirements**:
- Sort by CreatedAt (newest first/oldest first)
- Sort by Priority (High to Low, Low to High)
- Sort dropdown in toolbar
- Sort persists per session
- Real-time update on sort change

**Priority**: Must Have

#### FR5.3 Filter Tasks
**Description**: User must be able to filter tasks.

**Requirements**:
- Filter by priority (All, High, Medium, Low)
- Filter by completion status (All, Completed, Active)
- Show/hide completed toggle
- Filters combine (e.g., High priority + Completed)
- Filter indicators in toolbar

**Priority**: Must Have

#### FR5.4 Filter by Status (Board View)
**Description**: User must be able to filter tasks by status.

**Requirements**:
- Status filter dropdown (ToDo, InProgress, Done, All)
- Available in both List and Board views
- Filters update immediately

**Priority**: Must Have (Phase 2)

---

### FR6: Task Operations

#### FR6.1 Duplicate Task
**Description**: User must be able to duplicate tasks.

**Requirements**:
- Duplicate creates new task with same properties
- Title appended with "(Copy)"
- New GUID generated
- CreatedAt set to current time
- Subtasks are not duplicated
- Toast notification confirms action

**Priority**: Must Have

#### FR6.2 Move Task to Project
**Description**: User must be able to move tasks between projects.

**Requirements**:
- Project selector in task actions
- Moving task updates ProjectId
- Subtasks move with parent
- Task appears in new project immediately
- Task count badges update in both projects

**Priority**: Must Have

#### FR6.3 Clear All Completed
**Description**: User must be able to delete all completed tasks at once.

**Requirements**:
- "Clear All Completed" button in toolbar
- Shows count of tasks to be deleted
- Confirmation dialog
- Toast notification with UNDO option
- Undo restores all deleted completed tasks
- Subtasks of completed tasks also deleted

**Priority**: Must Have

---

### FR7: Keyboard Shortcuts

#### FR7.1 Global Shortcuts
**Description**: Application must support keyboard shortcuts.

**Requirements**:
| Shortcut | Action |
|----------|--------|
| Ctrl/Cmd + Enter | Complete selected task |
| Delete | Delete selected task |
| Ctrl/Cmd + N | Focus new task input |
| Ctrl/Cmd + F | Focus search input |
| Ctrl/Cmd + Z | Undo last action |
| Ctrl/Cmd + / | Show keyboard shortcuts help |

**Priority**: Must Have

#### FR7.2 Shortcut Help
**Description**: System must provide keyboard shortcut help.

**Requirements**:
- Modal shows all available shortcuts
- Opens with Ctrl/Cmd + /
- Closes with Escape or click outside

**Priority**: Must Have

---

### FR8: Notifications

#### FR8.1 Toast Notifications
**Description**: System must show toast notifications for actions.

**Requirements**:
- Success toast for completed tasks
- Info toast for duplicated/moved tasks
- Confirmation toast for deleted tasks
- Toast for "Clear All Completed"
- Auto-dismiss after 5 seconds
- Close button
- Undo button for destructive actions

**Priority**: Must Have

#### FR8.2 Undo Action
**Description**: Destructive actions must support undo.

**Requirements**:
- Undo available for: Delete, Clear All Completed
- Undo restores to previous state
- Undo available for 30 seconds
- Toast disappears after undo

**Priority**: Must Have

---

### FR9: User Interface

#### FR9.1 Dark Theme
**Description**: Application must use dark theme with Turquoise accent.

**Requirements**:
- Dark background (#1a1a2e)
- Card background (#16213e)
- Turquoise accent (#40E0D0)
- White primary text (#ffffff)
- Gray secondary text (#a0a0a0)
- High contrast (WCAG AA compliant)

**Priority**: Must Have

#### FR9.2 Responsive Design
**Description**: Application must be fully responsive.

**Requirements**:
- Desktop-first approach
- Breakpoints: >1024px (desktop), 768-1024px (tablet), <768px (mobile)
- Sidebar collapses on mobile (hamburger menu)
- Board columns stack on mobile
- All features accessible on mobile

**Priority**: Must Have

#### FR9.3 In-Line Editing
**Description**: Task editing must be in-line (no dialogs).

**Requirements**:
- Click task title to edit
- Edit on blur or Enter key
- Cancel on Escape key
- No dialog modals for editing
- No Save buttons for create/update flows; actions persist immediately

**Priority**: Must Have

#### FR9.4 Empty States
**Description**: System must show empty states when no content.

**Requirements**:
- Icon displayed
- Helpful message ("No tasks yet")
- Call-to-action button ("Create your first task")
- Different messages for different contexts (project, column, filter)

**Priority**: Must Have

---

### FR10: My Task Flow Views

#### FR10.1 My Task Flow Today View
**Description**: User must be able to see tasks that are due or marked for today.

**Requirements**:
- System displays tasks with due date = today
- System displays tasks marked as focused (IsFocused=true)
- Tasks shown from all projects or filtered by current project
- Tasks sorted by priority then due time
- View updates in real-time as time changes

**Priority**: Must Have

#### FR10.2 My Task Flow Upcoming View
**Description**: User must be able to see tasks due in the future.

**Requirements**:
- System displays tasks with due date > today
- System displays tasks without due dates (indefinite future)
- Tasks shown from all projects or filtered by current project
- Tasks sorted by due date ascending
- Tasks grouped by date (Today, Tomorrow, This Week, Later)

**Priority**: Must Have

#### FR10.3 My Task Flow Recent View
**Description**: User must be able to see recently created tasks.

**Requirements**:
- System displays recently created tasks (last 7 days)
- Tasks shown from all projects or filtered by current project
- Tasks sorted by CreatedAt descending (newest first)
- Badge shows "X new tasks added this week"

**Priority**: Must Have

#### FR10.4 View Switching
**Description**: User must be able to switch between project views and My Task Flow views.

**Requirements**:
- Sidebar includes "My Task Flow" section with Today, Upcoming, Recent options
- Clicking project shows that project's tasks
- Clicking My Task Flow section shows smart view
- Active view highlighted in sidebar
- View selection persists across sessions

**Priority**: Must Have

#### FR10.5 Task Mark for Today
**Description**: User must be able to mark any task for "today" without changing due date.

**Requirements**:
- Tasks can be marked as "due today" without specific due time
- Marked tasks appear in Today view
- Visual indicator shows task is marked for today
- Today mark is separate from specific due date/time
- Toggle on/off today mark

**Priority**: Must Have

---

### FR11: Accessibility

#### FR10.1 Keyboard Navigation
**Description**: All features must be accessible via keyboard.

**Requirements**:
- Tab navigation follows logical order
- All interactive elements keyboard-accessible
- Focus visible (outline or highlight)
- Skip to content link

**Priority**: Must Have

#### FR10.2 ARIA Labels
**Description**: All interactive elements must have ARIA labels.

**Requirements**:
- Buttons have aria-label
- Form inputs have aria-label
- Icons have aria-label
- Screen reader announcements for notifications

**Priority**: Must Have

#### FR10.3 High Contrast
**Description**: UI must meet WCAG AA contrast requirements.

**Requirements**:
- Minimum 4.5:1 contrast ratio for normal text
- Minimum 3:1 contrast ratio for large text
- Color not sole indicator of information

**Priority**: Must Have

---

### FR11: Import/Export

#### FR11.1 Export Projects
**Description**: User must be able to export selected projects to JSON format.

**Requirements**:
- User can select multiple projects to export
- Export includes project properties and all tasks (including subtasks)
- Export format is JSON with readable formatting
- Export includes IDs for import matching
- Exported JSON can be saved or copied

**Priority**: Should Have (Phase 3)

#### FR11.2 Import Projects
**Description**: User must be able to import projects from JSON format.

**Requirements**:
- User can upload JSON file or paste JSON content
- Import uses IDs to determine add vs update
- Existing IDs: Update existing projects and tasks
- New IDs: Create new projects and tasks
- Subtasks imported with parent task relationships
- Import updates project properties and all tasks
- Show confirmation with count of projects imported
- Handle import errors gracefully

**Priority**: Should Have (Phase 3)

#### FR11.3 Import/Export DTOs
**Description**: Data transfer objects must properly serialize domain entities.

**Requirements**:
- ProjectExportDto includes: Id, Name, Color, Icon, IsDefault, ViewType, CreatedAt, Tasks
- TaskExportDto includes: Id, Title, Note, Priority, IsCompleted, IsFocused, Status, CreatedAt, CompletedAt, SubTasks
- Proper enum serialization (Priority, Status, ViewType)
- Nested structure for subtasks
- JSON formatting is readable (indented)

**Priority**: Should Have (Phase 3)

---

### FR12: Database

#### FR12.1 Persistence
**Description**: All data must be persisted to SQLite database.

**Requirements**:
- Projects, tasks, SubTasks persisted
- All changes committed immediately
- Database file in named volume
- Database auto-creates on startup

**Priority**: Must Have

#### FR12.2 Seeding
**Description**: Database must be seeded with initial data.

**Requirements**:
- Default "Inbox" project
- 3 sample tasks with different priorities and notes
- 1 sample task with IsFocused=true
- All with CreatedAt timestamps

**Priority**: Must Have

#### FR12.3 Data Integrity
**Description**: Database must maintain data integrity.

**Requirements**:
- Foreign key constraints enforced
- Cascade delete for related entities
- GUID uniqueness enforced

**Priority**: Must Have

---

### FR13: Deployment

#### FR13.1 Docker Containers
**Description**: Application must run in Docker containers.

**Requirements**:
- Tailscale sidecar container
- Blazor Server application container
- Nginx reverse proxy container
- Named volume for database persistence

**Priority**: Must Have

#### FR13.2 Network Access
**Description**: Application must be accessible via Tailscale.

**Requirements**:
- URL: http://taskflow.churra-platy.ts.net
- Requires Tailscale network access
- HTTPS not required (private network)

**Priority**: Must Have

#### FR13.3 Monitoring
**Description**: Application must integrate with Uptime Kuma.

**Requirements**:
- Labels for monitoring
- Health check endpoint
- Container restart on failure

**Priority**: Must Have

---

## Non-Functional Requirements

### NFR1: Performance

#### NFR1.1 Response Time
**Requirement**: All user interactions must complete within 200ms for local network access.

**Priority**: Must Have

#### NFR1.2 Search Performance
**Requirement**: Search across 1000 tasks must complete within 500ms.

**Priority**: Must Have

#### NFR1.3 Page Load Time
**Requirement**: Initial page load must complete within 2 seconds.

**Priority**: Must Have

#### NFR1.4 Real-Time Updates
**Requirement**: UI updates must be received within 100ms of server-side change.

**Priority**: Must Have

---

### NFR2: Scalability

#### NFR2.1 User Scale
**Requirement**: System must support 1-3 concurrent users.

**Priority**: Must Have

#### NFR2.2 Data Scale
**Requirement**: System must support up to 1000 tasks with acceptable performance.

**Priority**: Must Have

#### NFR2.3 Project Scale
**Requirement**: System must support up to 50 projects.

**Priority**: Should Have

---

### NFR3: Reliability

#### NFR3.1 Availability
**Requirement**: System must be available 99% of the time.

**Priority**: Must Have

#### NFR3.2 Data Loss Prevention
**Requirement**: No data loss under normal operation.

**Priority**: Must Have

#### NFR3.3 Error Recovery
**Requirement**: System must recover from errors gracefully.

**Priority**: Must Have

---

### NFR4: Usability

#### NFR4.1 Learnability
**Requirement**: New users should be able to create tasks within 2 minutes without documentation.

**Priority**: Must Have

#### NFR4.2 Efficiency
**Requirement**: Common tasks (create, complete, delete) must require ≤ 2 clicks.

**Priority**: Must Have

#### NFR4.3 Satisfaction
**Requirement**: System must feel responsive and smooth (no perceptible lag).

**Priority**: Must Have

---

### NFR5: Maintainability

#### NFR5.1 Code Quality Standards
**Requirement**: Code must follow .NET coding standards and best practices.

**Requirements**:
- Methods should be < 50 lines of code
- Classes should be < 200 lines of code
- Maximum of 15 public methods per class
- Maximum of 10 properties per class
- Cyclomatic complexity < 10
- Async/await used consistently throughout
- Proper naming conventions (C# standards)
- Interface names prefixed with 'I'
- Async methods suffixed with 'Async'
- No code duplication (DRY principle)
- Single Responsibility Principle followed
- No async void (use Task.FromResult)

**Priority**: Must Have

#### NFR5.2 Documentation
**Requirement**: All public APIs and services must have XML documentation comments.

**Priority**: Should Have

#### NFR5.3 Testing
**Requirement**: Code must be testable and follow SOLID principles.

**Priority**: Should Have

---

### NFR6: Security

#### NFR6.1 Network Security
**Requirement**: Application must be accessible only via Tailscale VPN.

**Priority**: Must Have

#### NFR6.2 Input Validation
**Requirement**: All user input must be validated and sanitized.

**Priority**: Must Have

#### NFR6.3 SQL Injection Protection
**Requirement**: No SQL injection vulnerabilities (EF Core parameterized queries).

**Priority**: Must Have

#### NFR6.4 XSS Protection
**Requirement**: No XSS vulnerabilities (automatic Razor encoding).

**Priority**: Must Have

---

### NFR7: Compatibility

#### NFR7.1 Browser Support
**Requirement**: Must work on latest Chrome, Firefox, Safari, and Edge.

**Priority**: Must Have

#### NFR7.2 Mobile Support
**Requirement**: Must work on iOS Safari and Chrome Mobile.

**Priority**: Must Have

#### NFR7.3 Platform Support
**Requirement**: Must run on Raspberry Pi 5 ARM64 architecture.

**Priority**: Must Have

---

### FR13: Rich Domain Model

#### FR13.1 Rich Domain Entities
**Description**: Domain entities must encapsulate business logic and behavior.

**Requirements**:
- Task entity has methods: Complete(), AddSubTask(), MoveToProject(), SetPriority(), ToggleFocus(), UpdateTitle(), UpdateNote(), SetStatus()
- Project entity has methods: AddTask(), RemoveTask(), UpdateViewType(), GetTaskCount(), UpdateName(), UpdateColor(), UpdateIcon()
- Business rules enforced within entities
- Invariants protected by domain methods
- Domain state changes only through public methods
- Rich domain model (not anemic)

**Priority**: Must Have

#### FR13.2 Domain Aggregates
**Description**: Domain aggregates must manage consistency boundaries.

**Requirements**:
- Task is an aggregate root with SubTasks as children
- Project is an aggregate root with Tasks as children
- Aggregate roots control access to children
- Children modified only through aggregate root
- Consistency maintained within aggregate boundary
- Cross-aggregate operations handled by orchestrators

**Priority**: Must Have

#### FR13.3 Value Objects
**Description**: Value objects must be immutable types that represent concepts.

**Requirements**:
- TaskPriority enum (Low, Medium, High)
- TaskStatus enum (ToDo, InProgress, Done)
- ProjectViewType enum (List, Board)
- Value objects are immutable
- Value objects defined in domain layer
- No separate DTO classes

**Priority**: Must Have

#### FR13.4 Presentation Layer Uses Domain Directly
**Description**: Presentation layer must use domain aggregates without DTOs.

**Requirements**:
- No DTO classes created
- No mapping between entities and DTOs
- Domain aggregates (Task, Project) used directly in components
- Components call domain methods for business operations
- UI only manages display and user interaction

**Priority**: Must Have

#### FR13.5 Minimal Application Orchestrators
**Description**: Application layer must only orchestrate, not contain business logic.

**Requirements**:
- Orchestrators named TaskOrchestrator, ProjectOrchestrator (not BusinessService)
- Orchestrators only coordinate between repositories
- Orchestrators manage transactions
- Orchestrators handle cross-aggregate operations
- All business logic delegated to domain entities
- No business rules in orchestrators

**Priority**: Must Have

#### FR13.6 Repository Pattern
**Description**: Database access must be abstracted through repository interfaces.

**Requirements**:
- Repository interfaces (ITaskRepository, IProjectRepository) in domain layer
- Repository implementations in infrastructure layer
- Orchestrators depend on repository interfaces (not implementations)
- Repositories handle all data access operations
- Repositories can be mocked for unit testing
- No EF Core types exposed outside infrastructure layer

**Priority**: Must Have

#### FR13.7 Dependency Injection
**Description**: All dependencies must be injected through constructors.

**Requirements**:
- No service locator pattern
- All services use constructor injection
- Scoped services for Blazor Server circuits
- Interface-based registration in DI container
- No static dependencies on services

**Priority**: Must Have

#### FR13.8 No Business Services
**Description**: No separate business services should exist.

**Requirements**:
- No TaskBusinessService, ProjectBusinessService, etc.
- Business logic in domain entities
- Only minimal orchestrators in application layer
- Clear distinction between orchestration and business logic

**Priority**: Must Have

#### FR13.9 Component Reusability
**Description**: UI components must be reusable across views to avoid code duplication.

**Requirements**:
- TaskCard component reusable in List and Board views
- PriorityBadge component extracted and reused
- FocusButton component extracted and reused
- EmptyState component with customizable content
- FilterDropdown component extracted and reused
- Shared components in Presentation/Shared folder
- Components accept props for flexibility

**Priority**: Must Have

---

### FR14: Code Quality Standards

#### FR14.0 Immediate Persistence UX Rule
**Description**: Create and update actions must persist immediately without explicit save buttons.

**Requirements**:
- Any input change that confirms intent (blur, Enter, selection change, toggle, OK action) must write directly to persistence.
- No Save button in create/update forms and editors.
- Delete flows are the only flows that require explicit confirmation.
- Repository add/update methods are called as part of each confirmed UI action.

**Priority**: Must Have

#### FR14.1 Rich Domain Methods
**Description**: Domain entity methods must encapsulate business logic clearly.

**Requirements**:
- Each domain method has single responsibility
- Methods named with business verbs (Complete, AddSubTask, MoveToProject)
- Methods validate invariants before changing state
- Methods encapsulate related state changes
- Domain methods are synchronous (no async in domain logic)

**Priority**: Must Have

#### FR14.2 Method Complexity
**Description**: Methods must be simple and focused to maintain readability.

**Requirements**:
- Methods should be < 50 lines of code
- Cyclomatic complexity < 10
- Nesting depth < 4 levels
- Parameters limited to 5 or fewer
- Early return on simple conditions

**Priority**: Must Have

#### FR14.3 Class Complexity
**Description**: Classes must be focused and not grow too large.

**Requirements**:
- Domain entities can be larger (rich domain) but < 300 lines
- Orchestrators < 150 lines (minimal orchestration only)
- Maximum of 15 public methods per class
- Maximum of 10 properties per class
- Split large classes into smaller focused classes
- Single Responsibility Principle followed

**Priority**: Must Have

#### FR14.4 Async Consistency
**Description**: Async/await must be used consistently throughout the application.

**Requirements**:
- All repository methods async Task<T>
- All orchestrator methods async Task<T>
- No async void (use Task.FromResult for void methods)
- Proper async/await usage (no .Result on async methods)
- Domain entity methods are synchronous (no I/O in domain)

**Priority**: Must Have

#### FR14.5 Naming Conventions
**Description**: Code must follow consistent and clear naming conventions.

**Requirements**:
- C# naming conventions (PascalCase for classes, camelCase for methods/properties)
- Interface names prefixed with 'I'
- Async methods suffixed with 'Async'
- Boolean properties use 'Is' or 'Has' prefix
- Orchestrators named XyzOrchestrator (not XyzService or XyzBusinessService)
- Clear, descriptive names (no abbreviations)

**Priority**: Must Have

#### FR14.6 DRY Principle
**Description**: Code must avoid duplication through proper abstraction.

**Requirements**:
- Common logic extracted into reusable methods
- Similar code patterns extracted into base classes
- No copy-paste code duplication
- Shared components used for common UI patterns
- Utility methods for common operations

**Priority**: Must Have

---

## User Stories

### Epic 1: Project Management

**US1.1**: As a user, I want to create projects so that I can organize my tasks into meaningful groups.

**US1.2**: As a user, I want to view all projects in a sidebar so that I can quickly navigate between them.

**US1.3**: As a user, I want to edit project properties so that I can customize project appearance.

**US1.4**: As a user, I want to delete projects so that I can remove old or unused projects.

**US1.5**: As a user, I want a default "Inbox" project so that I can quickly add tasks without creating projects first.

---

### Epic 2: Task Management

**US2.1**: As a user, I want to create tasks so that I can track what needs to be done.

**US2.2**: As a user, I want to view tasks so that I can see what I need to work on.

**US2.3**: As a user, I want to edit tasks in-line so that I can quickly make changes without opening dialogs.

**US2.4**: As a user, I want to delete tasks so that I can remove completed or irrelevant tasks.

**US2.5**: As a user, I want to mark tasks as complete so that I can track my progress.

**US2.6**: As a user, I want to set priorities on tasks so that I can focus on what's important.

**US2.7**: As a user, I want to add notes to tasks so that I can include additional details.

**US2.8**: As a user, I want to pin tasks as focused so that they appear at the top.

**US2.9**: As a user, I want to see when tasks were created and completed so that I can track productivity.

---

### Epic 3: SubTask Management

**US3.1**: As a user, I want to create SubTasks so that I can break down complex tasks.

**US3.2**: As a user, I want to view SubTasks under parent tasks so that I can see task hierarchy.

**US3.3**: As a user, I want to edit SubTasks so that I can update SubTask details.

**US3.4**: As a user, I want to delete SubTasks so that I can remove unnecessary SubTasks.

**US3.5**: As a user, I want completing a parent task to complete all SubTasks so that I don't have to complete them individually.

---

### Epic 4: Board View

**US4.1**: As a user, I want to view tasks in a Kanban board so that I can visualize my workflow.

**US4.2**: As a user, I want to switch between List and Board views so that I can use the view that works best for me.

**US4.3**: As a user, I want to drag-and-drop tasks between columns so that I can update task status quickly.

**US4.4**: As a user, I want to add tasks directly to board columns so that I can create tasks in the right status.

---

### Epic 5: Search, Sort, Filter

**US5.1**: As a user, I want to search tasks by title so that I can quickly find specific tasks.

**US5.2**: As a user, I want to sort tasks so that I can view them in the order that makes sense to me.

**US5.3**: As a user, I want to filter tasks by priority and completion status so that I can focus on what matters.

**US5.4**: As a user, I want to filter tasks by status in board view so that I can see specific workflow stages.

---

### Epic 6: Task Operations

**US6.1**: As a user, I want to duplicate tasks so that I can quickly create similar tasks.

**US6.2**: As a user, I want to move tasks between projects so that I can reorganize my tasks.

**US6.3**: As a user, I want to clear all completed tasks so that I can clean up my lists efficiently.

---

### Epic 7: Keyboard Shortcuts

**US7.1**: As a power user, I want to use keyboard shortcuts so that I can work faster.

**US7.2**: As a power user, I want to see keyboard shortcut help so that I can learn available shortcuts.

---

### Epic 8: Notifications

**US8.1**: As a user, I want to see toast notifications so that I know when my actions succeed.

**US8.2**: As a user, I want to undo destructive actions so that I can recover from mistakes.

---

### Epic 9: User Interface

**US9.1**: As a user, I want a dark theme with Turquoise accent so that the app looks modern and easy on the eyes.

**US9.2**: As a user, I want the app to work on my phone so that I can access tasks anywhere.

**US9.3**: As a user, I want to edit tasks in-line so that I can make changes quickly without interruptions.

**US9.4**: As a user, I want helpful empty states so that I know what to do when there's no content.

---

### Epic 10: Accessibility

**US10.1**: As a keyboard user, I want to navigate the entire app with keyboard so that I can work efficiently.

**US10.2**: As a screen reader user, I want all elements to have ARIA labels so that I can use the app.

**US10.3**: As a user with low vision, I want high contrast so that I can read the text easily.

---

### Epic 11: My Task Flow

**US11.1**: As a user, I want to see a "Today" view so that I can focus on tasks due today.

**US11.2**: As a user, I want to see an "Upcoming" view so that I can plan for the future.

**US11.3**: As a user, I want to see a "Recent" view so that I can quickly access new tasks.

**US11.4**: As a user, I want to switch between project views and My Task Flow views so that I can choose the best perspective.

**US11.5**: As a user, I want to mark tasks for "today" without setting a specific time so that I can quickly organize my day.

---

### Epic 12: Focus Timer (Future - Phase 3)

**US12.1**: As a user, I want a focus timer to help me stay concentrated on a task.

**US12.2**: As a user, I want to configure the timer (duration, alerts) to match my work style.

---

### Epic 13: Clean Architecture

**US13.1**: As a developer, I want the application to follow clean architecture principles so that it is maintainable and testable.

**US13.2**: As a developer, I want services to use interfaces so that they can be mocked for unit testing.

**US13.3**: As a developer, I want components to be reusable and composable so that I don't duplicate code.

**US13.4**: As a developer, I want database access to be abstracted through repository pattern so that data logic is separated.

**US13.5**: As a developer, I want domain logic to be separated from application logic so that business rules are centralized.

---

## Acceptance Criteria

### AC1: Project Management

**AC1.1 Create Project**
- GIVEN I am on the Projects page
- WHEN I enter a project name, select a color and icon, and click "Create Project"
- THEN the project is created with a unique GUID
- AND the project appears in the sidebar
- AND the project has the specified name, color, and icon
- AND the CreatedAt timestamp is set to current time

**AC1.2 View Projects**
- GIVEN I have multiple projects
- WHEN I look at the sidebar
- THEN all projects are displayed
- AND each project shows icon, name, and task count
- AND the active project is highlighted
- AND projects are sorted by creation date

**AC1.3 Delete Project**
- GIVEN I have a non-default project with tasks
- WHEN I click "Delete Project" and confirm
- THEN the project is deleted
- AND all tasks in the project are deleted
- AND a toast notification appears with UNDO option
- AND clicking UNDO restores the project and all tasks
- AND the system selects another project automatically

---

### AC2: Task Management

**AC2.1 Create Task**
- GIVEN I am viewing a project
- WHEN I enter a task title and press Enter
- THEN a new task is created with a unique GUID
- AND the task appears in the list
- AND the task has Medium priority by default
- AND the CreatedAt timestamp is set

**AC2.2 Complete Task**
- GIVEN I have an active task
- WHEN I click the task's checkbox
- THEN the task is marked as complete
- AND the CompletedAt timestamp is set
- AND the task appears with strikethrough styling
- AND a success toast notification appears

**AC2.3 Edit Task**
- GIVEN I have a task
- WHEN I click the task title
- THEN the title becomes an editable input
- AND I can edit the text
- AND changes are saved when I press Enter or click away
- AND the updated title persists to the database

**AC2.4 Delete Task**
- GIVEN I have a task
- WHEN I click the delete button
- THEN the task is deleted
- AND a toast notification appears with UNDO option
- AND clicking UNDO restores the task

**AC2.5 Filter Tasks**
- GIVEN I have tasks with different priorities
- WHEN I select "High" in the priority filter
- THEN only High priority tasks are displayed
- AND the filter indicator is visible
- AND filtering combines with other filters

**AC2.6 Sort Tasks**
- GIVEN I have multiple tasks
- WHEN I select "Priority" in the sort dropdown
- THEN tasks are sorted by priority (High to Low)
- AND the sort persists across the session

---

### AC3: Subtask Management

**AC3.1 Create Subtask**
- GIVEN I have a task
- WHEN I click "Add Subtask" and enter a title
- THEN a new subtask is created under the parent
- AND the subtask has ParentTodoId set to the parent
- AND the parent's subtask count badge increments

**AC3.2 Auto-Complete Subtasks**
- GIVEN I have a task with subtasks
- WHEN I complete the parent task
- THEN all subtasks are marked as complete
- AND the CompletedAt timestamps are set on subtasks

---

### AC4: Board View

**AC4.1 View Tasks in Board**
- GIVEN I have a project in Board view
- WHEN I view the project
- THEN tasks are displayed in 3 columns (ToDo, InProgress, Done)
- AND each column shows a task count
- AND I can see task cards in appropriate columns

**AC4.2 Drag-and-Drop**
- GIVEN I have a task in the ToDo column
- WHEN I drag the task to the InProgress column
- THEN the task's Status is updated to InProgress
- AND the task moves to the new column
- AND the task counts update

**AC4.3 Switch Views**
- GIVEN I have a project in List view
- WHEN I click "Board" in the view switcher
- THEN the view changes to Board
- AND all tasks appear in appropriate columns
- AND the view type persists for the project

---

### AC5: Keyboard Shortcuts

**AC5.1 Complete with Shortcut**
- GIVEN I have a task selected
- WHEN I press Ctrl/Cmd + Enter
- THEN the task is marked as complete
- AND the same behavior as clicking the checkbox occurs

**AC5.2 Delete with Shortcut**
- GIVEN I have a task selected
- WHEN I press Delete
- THEN the task is deleted
- AND a toast notification appears with UNDO option

**AC5.3 Focus New Task**
- GIVEN I am viewing tasks
- WHEN I press Ctrl/Cmd + N
- THEN the new task input is focused
- AND I can immediately start typing

---

### AC6: Notifications

**AC6.1 Toast Notification**
- GIVEN I perform an action (complete, delete, duplicate, move)
- WHEN the action completes
- THEN a toast notification appears
- AND the notification shows appropriate message
- AND the notification auto-dismisses after 5 seconds

**AC6.2 Undo Action**
- GIVEN I deleted a task
- WHEN I click UNDO in the toast notification
- THEN the task is restored
- AND the task reappears in the list
- AND the toast notification disappears

---

## Constraints

### C1: Technical Constraints

**C1.1 Platform**
- Must run on Raspberry Pi 5 (ARM64)
- Must use .NET 10.0
- Must use Blazor Server
- Must use SQLite database
- Must use Docker containers

**C1.2 Access**
- Must be accessible only via Tailscale VPN
- URL: http://taskflow.churra-platy.ts.net
- No public internet access

**C1.3 Architecture**
- Single Blazor Server project (no separate API)
- Direct EF Core access (no REST endpoints)
- SignalR for real-time updates

---

### C2: Budget Constraints

**C2.1 Infrastructure**
- Use existing Raspberry Pi 5
- No additional hardware costs
- No cloud services

**C2.2 Software**
- Use free and open-source software
- No paid licenses

---

### C3: Time Constraints

**C3.1 Implementation**
- Estimated time: 6-8 hours
- Phase 1: 3-4 hours
- Phase 2: 2-3 hours
- Phase 3: 1-2 hours (Focus Timer)

**C3.2 Learning**
- No formal training required
- Use existing skills and documentation

---

### C4: User Constraints

**C4.1 Scale**
- Single user deployment
- No multi-user support required
- No authentication required

**C4.2 Expertise**
- User is technically proficient
- Comfortable with CLI and configuration
- Familiar with task management apps

---

## Future Enhancements

### FE1: Authentication & Authorization
- Add user login (ASP.NET Core Identity)
- Multi-user support
- Role-based access control
- Per-user data isolation

### FE2: Collaboration
- Share projects with other users
- Real-time collaboration on tasks
- Comments and mentions
- Activity feed

### FE3: Due Dates & Reminders
- Add due date field to tasks
- Due date picker
- Date-based filtering
- Notifications for due tasks
- Recurring tasks

### FE4: Attachments
- File attachments to tasks
- Image preview
- Document preview
- Link attachments

### FE5: Tags & Labels
- Add tags to tasks
- Color-coded tags
- Filter by tags
- Tag autocomplete

### FE6: Project Templates
- Pre-built project templates
- Custom templates
- Template marketplace

### FE7: Advanced Analytics
- Task completion statistics
- Productivity charts
- Time tracking
- Export reports

### FE8: Integrations
- Calendar integration (Google, Outlook)
- Email integration (create tasks from email)
- Zapier/IFTTT integration
- Webhooks

### FE9: Mobile App
- Native iOS app
- Native Android app
- Offline support
- Push notifications

### FE10: Advanced Board Features
- Swimlanes
- Multiple board layouts
- Custom columns
- Column limits (WIP limits)
- Automated workflows

### FE11: Advanced Subtask Features
- Multi-level SubTasks (nested)
- SubTask dependencies
- SubTask progress tracking
- SubTask templates

### FE12: Focus Timer (Phase 3)
- Pomodoro-style timer (25/5/15/5/25 cycles)
- Customizable timer durations
- Sound notifications for timer events
- Visual countdown display
- Timer history and statistics
- Focus session tracking
- Break reminders
- Strict mode vs. flexible mode (snooze option)

### FE13: Advanced Search
- Full-text search
- Search across projects
- Saved searches
- Search operators (AND, OR, NOT)

### FE13: Custom Views
- Saved filters
- Custom dashboards
- Widgets
- Drag-and-drop widget builder

### FE14: Task History
- Full audit log
- Compare versions
- Restore previous versions
- Activity timeline

### FE15: Advanced Keyboard Shortcuts
- Customizable shortcuts
- Macro recording
- Quick actions menu
- Command palette

---

## Requirements Traceability Matrix

| Requirement ID | User Story | Acceptance Criteria | Priority |
|----------------|------------|---------------------|----------|
| FR1.1 | US1.1 | AC1.1 | Must Have |
| FR1.2 | US1.2 | AC1.2 | Must Have |
| FR1.3 | US1.3 | - | Must Have |
| FR1.4 | US1.4 | AC1.3 | Must Have |
| FR1.5 | US1.5 | - | Must Have |
| FR2.1 | US2.1 | AC2.1 | Must Have |
| FR2.2 | US2.2 | - | Must Have |
| FR2.3 | US2.3 | AC2.3 | Must Have |
| FR2.4 | US2.4 | AC2.4 | Must Have |
| FR2.5 | US2.5 | AC2.2 | Must Have |
| FR2.6 | US2.6 | - | Must Have |
| FR2.7 | US2.7 | - | Must Have |
| FR2.8 | US2.8 | - | Must Have |
| FR2.9 | US2.9 | - | Must Have |
| FR3.1 | US3.1 | AC3.1 | Must Have (Phase 2) |
| FR3.2 | US3.2 | - | Must Have (Phase 2) |
| FR3.3 | US3.3 | - | Must Have (Phase 2) |
| FR3.4 | US3.4 | - | Must Have (Phase 2) |
| FR3.5 | US3.5 | AC3.2 | Must Have (Phase 2) |
| FR4.1 | US4.1 | AC4.1 | Must Have (Phase 2) |
| FR4.2 | US4.2 | AC4.3 | Must Have (Phase 2) |
| FR4.3 | US4.3 | AC4.2 | Must Have (Phase 2) |
| FR4.4 | US4.4 | - | Must Have (Phase 2) |
| FR5.1 | US5.1 | - | Must Have |
| FR5.2 | US5.2 | AC2.6 | Must Have |
| FR5.3 | US5.3 | AC2.5 | Must Have |
| FR5.4 | US5.4 | - | Must Have (Phase 2) |
| FR6.1 | US6.1 | - | Must Have |
| FR6.2 | US6.2 | - | Must Have |
| FR6.3 | US6.3 | - | Must Have |
| FR7.1 | US7.1 | AC5.1, AC5.2, AC5.3 | Must Have |
| FR7.2 | US7.2 | - | Must Have |
| FR8.1 | US8.1 | AC6.1 | Must Have |
| FR8.2 | US8.2 | AC6.2 | Must Have |
| FR9.1 | US9.1 | - | Must Have |
| FR9.2 | US9.2 | - | Must Have |
| FR9.3 | US9.3 | AC2.3 | Must Have |
| FR9.4 | US9.4 | - | Must Have |
| FR10.1 | US10.1 | - | Must Have |
| FR10.2 | US10.2 | - | Must Have |
| FR10.3 | US10.3 | - | Must Have |
| FR10.1 | US11.1 | - | Must Have |
| FR10.2 | US11.2 | - | Must Have |
| FR10.3 | US11.3 | - | Must Have |
| FR10.4 | US11.4 | - | Must Have |
| FR10.5 | US11.5 | - | Must Have |
| FR11.1 | US12.1 | - | Must Have |
| FR11.2 | US12.2 | - | Must Have |
| FR11.3 | US12.3 | - | Must Have |
| FR12.1 | US13.1 | - | Must Have |
| FR12.2 | US13.2 | - | Must Have |
| FR12.3 | US13.3 | - | Must Have |

---

## Appendix

### A. Definitions

- **Project**: A collection of related tasks, grouped together for organization
- **Task**: An item to be completed, with title, priority, status, and metadata
- **Subtask**: A nested task under a parent task (1-level only)
- **Priority**: Importance level (Low=1, Medium=2, High=3)
- **Status**: Workflow state (ToDo, InProgress, Done)
- **Board View**: Kanban-style layout with 3 columns
- **List View**: Traditional vertical list layout
- **My Task Flow**: Smart task views (Today, Upcoming, Recent) that aggregate tasks across all projects
- **Today View**: Tasks due today or marked for today (independent of projects)
- **Upcoming View**: Tasks with due dates in future, grouped by time periods
- **Recent View**: Recently created tasks (last 7 days), sorted by creation date
- **Focus Pin**: Marker to highlight and prioritize important tasks
- **Toast Notification**: Temporary popup notification with auto-dismiss
- **In-Line Editing**: Editing content directly in place without dialogs

### B. Acronyms

- **EF Core**: Entity Framework Core
- **GUID**: Globally Unique Identifier
- **MVC**: Model-View-Controller
- **RAZ**: Razor Components
- **SQL**: Structured Query Language
- **WCAG**: Web Content Accessibility Guidelines
- **XSS**: Cross-Site Scripting

### C. References

- .NET 10.0 Documentation: https://docs.microsoft.com/dotnet/
- Blazor Documentation: https://docs.microsoft.com/aspnet/core/blazor/
- MudBlazor Documentation: https://mudblazor.com/
- Entity Framework Core Documentation: https://docs.microsoft.com/ef/core/
- SQLite Documentation: https://www.sqlite.org/docs.html
- WCAG 2.1 Guidelines: https://www.w3.org/WAI/WCAG21/quickref/

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 2.0 | 2025-02-09 | System | Added FR13 Clean Architecture (8 requirements), added FR14 Code Quality Standards (5 requirements), updated NFR5 to include code quality standards, updated system design with clean architecture 4-layer onion |
| 1.2 | 2025-02-09 | System | Added My Task Flow concept (Today, Upcoming, Recent), added Focus Timer as Phase 3, updated terminology (Todo→Task, subtask→SubTask), added Note and IsFocused fields |
| 1.1 | 2025-02-09 | System | Updated terminology (Todo→Task, subtask→SubTask), added Note and IsFocused fields |
| 1.0 | 2025-02-09 | System | Initial requirements document |

---

*Document Last Updated: February 9, 2026*
