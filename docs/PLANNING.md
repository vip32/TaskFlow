# TaskFlow - Implementation Planning Document

## Table of Contents
1. [Project Overview](#project-overview)
2. [Phase Breakdown](#phase-breakdown)
3. [Detailed Task Lists](#detailed-task-lists)
4. [Implementation Dependencies](#implementation-dependencies)
5. [Risk Assessment](#risk-assessment)
6. [Testing Approach](#testing-approach)
7. [Deployment Plan](#deployment-plan)
8. [Rollback Plan](#rollback-plan)

---

## Project Overview

### Summary
TaskFlow is a personal task management web application built with Blazor Server, providing users with project organization, task management with priorities and notes, subtasks, Kanban board views, and smart "My Task Flow" views.

### Technical Stack
- **Framework**: .NET 10.0 (ASP.NET Core)
- **Runtime**: Blazor Server
- **Database**: SQLite with Entity Framework Core
- **UI Framework**: MudBlazor 8.x
- **Deployment**: Docker containers on Raspberry Pi 5 (ARM64)
- **Network**: Tailscale VPN for private access
- **Architecture**: Domain-Driven Design (DDD) with Rich Domain Model

### Project Location
- **Source**: `/home/vip32/Projects/TaskFlow/`
- **Database Volume**: `taskflow-data`
- **Tailscale State**: `/home/vip32/Projects/ts-taskflow/state`
- **Nginx Config**: `/home/vip32/Projects/taskflow-nginx/nginx.conf`

### Access URL
- **Production**: http://taskflow.churra-platy.ts.net

### Execution Status (2026-02-09)

#### Completed
- [x] Solution scaffolded (`TaskFlow.sln`) with all baseline projects in `src/` and `tests/`.
- [x] Project references configured according to architecture dependency direction.
- [x] Core packages added (EF Core, SQLite provider, MudBlazor, test stack).
- [x] Domain baseline implemented for Phase 0B:
  - [x] `Project` aggregate
  - [x] `Task` aggregate
  - [x] Value enums (`TaskPriority`, `TaskStatus`, `ProjectViewType`)
  - [x] Repository interfaces (`IProjectRepository`, `ITaskRepository`)
  - [x] Optional `FocusSession` aggregate
- [x] Build and test verified locally via .NET 10 SDK.
- [x] CI workflow created for restore/build/test/container build (no image push).
- [x] Dockerfile added for application container build.
- [x] MinVer local tool manifest added and MinVer wired into build.
- [x] Central package management enabled (`Directory.Packages.props`).
- [x] Shared build settings centralized (`Directory.Build.props`).

#### In Progress / Deferred
- [x] Phase 0C Infrastructure implementation (DbContext, repositories, migrations, seeding).
- [x] Phase 0D Application orchestrators.
- [x] Phase 0E DI registration and startup composition baseline (application and infrastructure registrations wired in `Program.cs`).
- [ ] Tailscale-related deployment steps are intentionally deferred for now.

---

## Phase Breakdown

### Phase 0: Rich Domain Model Setup (1 hour)
**Goal**: Implement rich domain model with entities containing business logic and minimal application orchestration services.

**Scope**:
- 4-layer architecture (Presentation, Application, Domain, Infrastructure)
- Rich domain entities with behavior methods
- Domain aggregates (Project, Task with SubTasks)
- Value objects (Priority, Status, ProjectViewType)
- Repository interfaces and implementations
- Minimal application services for orchestration only
- No DTOs - domain aggregates used directly
- Reusable UI components (TaskCard, PriorityBadge, FocusButton, EmptyState, FilterDropdown)

**Deliverables**:
- Rich domain model established
- Domain entities with business logic methods
- Repository pattern implemented
- Minimal application services for orchestration
- Component reusability implemented
- All layers properly separated
- DI container configured

### Phase 1: Core Features (3-4 hours)
**Goal**: Implement basic project and task management with advanced UX features.

**Scope**:
- Project CRUD operations
- Task CRUD operations
- Task notes and priority
- Task focus pins
- Search, sort, filter
- Keyboard shortcuts
- Toast notifications with undo
- Dark theme with Turquoise accent
- Responsive design

**Deliverables**:
- Working TaskFlow application with full task management
- All CRUD operations functional
- All UX features implemented
- Docker containers deployed

### Phase 2: Board Views + SubTasks (2-3 hours)
**Goal**: Add Kanban board layout and subtask support.

**Scope**:
- Board view with 3 columns
- Drag-and-drop between columns
- SubTask CRUD operations
- SubTask indentation and hierarchy
- Auto-complete SubTasks when parent completes
- Per-project view switching (List ↔ Board)

**Deliverables**:
- Functional Kanban board
- SubTask support in both views
- View switcher working

### Phase 3: My Task Flow + Focus Timer (1-2 hours)
**Goal**: Add smart task views and focus timer for productivity.

**Scope**:
- My Task Flow views (Today, Upcoming, Recent)
- Task marking for "today"
- Focus timer (Pomodoro-style)
- Timer configuration (duration, alerts)
- Timer history tracking

**Deliverables**:
- My Task Flow views implemented
- Focus timer functional
- Task organization across all views

---

## Detailed Task Lists

### Phase 1: Core Features (98 tasks)

#### Phase 0A: Project Setup (7 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 0.1 | Create directories: `/home/vip32/Projects/ts-taskflow/state`, `/home/vip32/Projects/taskflow-nginx` | 2 min | - | Pending |
| 0.2 | Generate Blazor Server project at `/home/vip32/Projects/taskflow` | 5 min | - | Pending |
| 0.3 | Add MudBlazor package to project | 2 min | 0.2 | Pending |
| 0.4 | Add EF Core SQLite package | 2 min | 0.3 | Pending |
| 0.5 | Add EF Core Design package | 2 min | 0.4 | Pending |
| 0.6 | Consult blazor-expert skill for component architecture | 5 min | 0.3 | Pending |
| 0.7 | Verify all packages installed correctly | 2 min | 0.3-0.5 | Pending |

**Phase 0A Checklist**:
- [ ] Directory structure created
- [ ] Blazor Server project generated
- [ ] NuGet packages installed: MudBlazor, EF Core SQLite, EF Core Design
- [ ] Project builds successfully
- [ ] Consulted blazor-expert for best practices

### Phase 0B: Rich Domain Model Setup (10 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 0.8 | Create `Domain/` folder with all domain types (flat, no subfolders) | 10 min | 0.2 | Pending |
| 0.9 | Create `Domain/Project.cs` aggregate with business methods | 10 min | 0.8 | Pending |
| 0.10 | Create `Domain/Task.cs` aggregate with business methods | 15 min | 0.8 | Pending |
| 0.11 | Create `Domain/TaskPriority.cs` value object (enum) | 5 min | 0.8 | Pending |
| 0.12 | Create `Domain/TaskStatus.cs` value object (enum) | 5 min | 0.8 | Pending |
| 0.13 | Create `Domain/ProjectViewType.cs` value object (enum) | 5 min | 0.8 | Pending |
| 0.14 | Add domain methods to Task: Complete(), AddSubTask(), MoveToProject(), SetPriority(), ToggleFocus(), UpdateTitle(), UpdateNote(), SetStatus() | 15 min | 0.10 | Pending |
| 0.15 | Add domain methods to Project: AddTask(), RemoveTask(), UpdateViewType(), CalculateTaskCount() | 10 min | 0.9 | Pending |
| 0.16 | Create `Domain/ITaskRepository.cs` | 5 min | 0.10 | Pending |
| 0.17 | Create `Domain/IProjectRepository.cs` | 5 min | 0.9 | Pending |
| 0.18 | Create optional `Domain/FocusSession.cs` aggregate (Phase 3) | 5 min | 0.8 | Pending |

**Phase 0B Checklist**:
- [ ] Domain folder created (flat, no subfolders)
- [ ] Domain entities created with business logic methods
- [ ] Value objects defined as immutable types (enums)
- [ ] Task aggregate: Complete(), AddSubTask(), MoveToProject(), SetPriority(), ToggleFocus(), UpdateTitle(), UpdateNote(), SetStatus()
- [ ] Project aggregate: AddTask(), RemoveTask(), UpdateViewType(), GetTaskCount(), UpdateName(), UpdateColor(), UpdateIcon()
- [ ] Repository interfaces created
- [ ] Domain follows SOLID principles
- [ ] FocusSession aggregate created (Phase 3)

#### Phase 0C: Infrastructure Layer Setup (10 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 0.19 | Create `Infrastructure/Data/AppDbContext.cs` with DbSet properties | 10 min | 0.10, 0.9 | Pending |
| 0.20 | Create `Infrastructure/Data/TaskRepository.cs` implementing ITaskRepository | 15 min | 0.16 | Pending |
| 0.21 | Create `Infrastructure/Data/ProjectRepository.cs` implementing IProjectRepository | 10 min | 0.17 | Pending |
| 0.22 | Create `Infrastructure/Data/AppDbContextSeed.cs` for data seeding | 15 min | 0.19 | Pending |
| 0.23 | Create `Infrastructure/Configuration.cs` | 5 min | 0.19 | Pending |
| 0.24 | Add seeding logic: 1 default project, 3 sample tasks with notes and focused task | 10 min | 0.22 | Pending |
| 0.25 | Configure SQLite connection string | 5 min | 0.23 | Pending |
| 0.26 | Test database operations | 10 min | 0.20, 0.21 | Pending |
| 0.27 | Create `Infrastructure/Data/FocusSessionRepository.cs` (Phase 3) | 5 min | 0.18 | Pending |

**Phase 0C Checklist**:
- [ ] AppDbContext configured with DbSets
- [ ] TaskRepository implemented (flat in Infrastructure/Data, no Repositories subfolder)
- [ ] ProjectRepository implemented (flat in Infrastructure/Data, no Repositories subfolder)
- [ ] Seeding logic created and functional
- [ ] Configuration created (flat in Infrastructure)
- [ ] SQLite connection working
- [ ] Database operations tested
- [ ] FocusSessionRepository created (Phase 3)
- [ ] FocusSessionRepository created (Phase 3)

#### Phase 0D: Application Layer Setup (Orchestration Only) (8 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 0.28 | Create `Application/` folder with feature slice structure | 10 min | 0.3 | Pending |
| 0.29 | Create `Application/IProjectOrchestrator.cs` for orchestration | 10 min | 0.28 | Pending |
| 0.30 | Create `Application/ITaskOrchestrator.cs` for orchestration | 10 min | 0.28 | Pending |
| 0.31 | Create `Application/ProjectOrchestrator.cs` - minimal orchestration | 15 min | 0.29 | Pending |
| 0.32 | Create `Application/TaskOrchestrator.cs` - minimal orchestration | 20 min | 0.30 | Pending |
| 0.33 | Add CreateProjectAsync to ProjectOrchestrator (calls repository + domain) | 5 min | 0.31 | Pending |
| 0.34 | Add CreateTaskAsync to TaskOrchestrator (calls repository + domain) | 10 min | 0.32 | Pending |
| 0.35 | Add GetTasksByProjectAsync to TaskOrchestrator (queries only) | 5 min | 0.32 | Pending |
| 0.36 | Add orchestration for cross-aggregate operations (e.g., MoveTaskBetweenProjects) | 10 min | 0.32 | Pending |

**Phase 0D Checklist**:
- [ ] Application folder with feature slice structure created
- [ ] IProjectOrchestrator interface created
- [ ] ITaskOrchestrator interface created
- [ ] ProjectOrchestrator implemented (minimal, orchestration only)
- [ ] TaskOrchestrator implemented (minimal, orchestration only)
- [ ] Orchestrators delegate business logic to domain entities
- [ ] Orchestrators handle transactions and repository coordination
- [ ] No business logic in orchestrators

#### Phase 0E: Configuration & DI Setup (5 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 0.38 | Configure DI container in `Program.cs` - register repositories as scoped | 10 min | 0.20, 0.21 | Pending |
| 0.39 | Configure DI container - register application services as scoped | 10 min | 0.32, 0.33 | Pending |
| 0.40 | Configure DbContextFactory in DI container | 5 min | 0.27 | Pending |
| 0.41 | Configure Blazor Server with SignalR | 5 min | 0.27, 0.40 | Pending |
| 0.42 | Configure MudBlazor services in `Program.cs` | 10 min | 0.3, 0.40 | Pending |

**Phase 0E Checklist**:
- [ ] Repositories registered as scoped
- [ ] Application services registered as scoped
- [ ] DbContextFactory registered
- [ ] Blazor Server configured with SignalR
- [ ] MudBlazor theme configured with custom Turquoise colors

#### Phase 0F: Test Architecture Setup (5 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 0.49 | Test all dependency injections work correctly | 5 min | 0.44 | Pending |
| 0.50 | Test repository methods work | 10 min | 0.19, 0.20 | Pending |
| 0.51 | Test application services coordinate correctly | 10 min | 0.29, 0.30 | Pending |
| 0.52 | Test database operations through full layer stack | 10 min | 0.25 | Pending |
| 0.53 | Test clean architecture separation (no cross-layer violations) | 5 min | 0.26, 0.46 | Pending |

**Phase 0F Checklist**:
- [ ] Dependency injection working correctly
- [ ] Repository operations functional
- [ ] Application services functional
- [ ] Database operations work through all layers
- [ ] Clean architecture separation verified

### Phase 0 Summary
- **Total Tasks**: 58
- **Total Estimated Time**: 1.5 hours
- **Dependencies**: None (Phase 0 is foundation)
- **Deliverables**: Rich domain model, repository pattern, minimal application services, feature-sliced structure

---

### Phase 1: Full Solution Setup + Core Features (4-5 hours)

#### Phase 1A: Solution Structure Setup (15 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 1.1 | Create full solution structure with all project folders | 15 min | Phase 0 | Pending |
| 1.2 | Create `Domain/` folder (flat, all types in one folder) | 5 min | 1.1 | Pending |
| 1.3 | Create `Application/` folder with feature slices | 10 min | 1.1 | Pending |
| 1.4 | Create `Infrastructure/` folder structure | 10 min | 1.1 | Pending |
| 1.5 | Create `Presentation/` folder with feature slices | 10 min | 1.1 | Pending |
| 1.6 | Set up `TaskFlow.sln` solution file | 5 min | 1.1 | Pending |
| 1.7 | Configure project references (solution-level dependencies) | 10 min | 1.6 | Pending |
| 1.8 | Set up `Directory.Build.props` for shared MSBuild properties | 5 min | 1.6 | Pending |
| 1.9 | Verify solution builds successfully | 5 min | 1.7 | Pending |
| 1.10 | Create GitHub repository and initialize | 5 min | 1.9 | Pending |
| 1.11 | Create `.github/workflows/docker-build.yml` workflow | 15 min | 1.10 | Pending |
| 1.12 | Configure workflow to build on push to main | 5 min | 1.11 | Pending |
| 1.13 | Configure workflow to build Docker image (ARM64) | 10 min | 1.11 | Pending |
| 1.14 | Configure workflow to push image to container registry | 10 min | 1.13 | Pending |
| 1.15 | Test GitHub Actions workflow manually | 10 min | 1.14 | Pending |

**Phase 1A Checklist**:
- [ ] Full solution structure created
- [ ] Domain folder flat (no subfolders)
- [ ] Application folder feature-sliced
- [ ] Infrastructure folder properly structured
- [ ] Presentation folder feature-sliced
- [ ] Solution file created and configured
- [ ] Project references configured
- [ ] Directory.Build.props created
- [ ] Solution builds successfully
- [ ] GitHub repository initialized
- [ ] GitHub Actions workflow created
- [ ] Workflow configured for builds on push
- [ ] Workflow builds ARM64 Docker image
- [ ] Workflow pushes to container registry
- [ ] Workflow tested manually

#### Phase 1B: Infrastructure Layer Setup (10 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 1.8 | Create `Models/Project.cs` with Guid Id | 10 min | 1.7 | Pending |
| 1.9 | Create `Models/Task.cs` with Guid Id, Note, IsFocused | 15 min | 1.7 | Pending |
| 1.10 | Create `Models/Task.cs` - Add Status and ParentTaskId for Phase 2 | 5 min | 1.9 | Pending |
| 1.11 | Create `Data/AppDbContext.cs` with Project and Task DbSets | 15 min | 1.8, 1.9 | Pending |
| 1.12 | Add seeding: 1 default project "Inbox", 3 sample tasks with notes and focused task | 10 min | 1.11 | Pending |
| 1.13 | Configure SQLite connection in `appsettings.json` | 5 min | - | Pending |
| 1.14 | Register DbContext and services in `Program.cs` | 10 min | 1.11, 1.13 | Pending |
| 1.15 | Configure Blazor Server with SignalR | 5 min | 1.14 | Pending |
| 1.16 | Ensure database auto-creates on startup | 5 min | 1.14 | Pending |
| 1.17 | Configure MudBlazor services in `Program.cs` | 10 min | 1.3 | Pending |
| 1.18 | Add keyboard shortcut handler service | 10 min | 1.17 | Pending |
| 1.19 | Test database seeding on startup | 5 min | 1.16 | Pending |

**Phase 1B Checklist**:
- [ ] Project model created with all required properties
- [ ] Task model created with Note and IsFocused fields
- [ ] Task model has Status and ParentTaskId for Phase 2
- [ ] DbContext configured with DbSet<Project> and DbSet<Task>
- [ ] Database seeding working (default project + sample tasks)
- [ ] SQLite connection configured
- [ ] Services registered in DI container
- [ ] Blazor Server configured with SignalR
- [ ] Database auto-creates on first run
- [ ] MudBlazor theme configured with custom Turquoise colors
- [ ] Keyboard service added

#### Phase 1C: Services Layer (15 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 1.20 | Create `Services/ProjectService.cs` with CRUD methods | 20 min | 1.11 | Pending |
| 1.21 | Create `Services/ProjectService.cs` - Add GetTaskCount method | 5 min | 1.20 | Pending |
| 1.22 | Create `Services/TaskService.cs` with CRUD methods | 25 min | 1.11 | Pending |
| 1.23 | Create `Services/TaskService.cs` - Add GetByProjectAsync with filters | 10 min | 1.22 | Pending |
| 1.24 | Create `Services/TaskService.cs` - Add GetByPriorityAsync | 5 min | 1.22 | Pending |
| 1.25 | Create `Services/TaskService.cs` - Add SearchAsync method | 10 min | 1.22 | Pending |
| 1.26 | Create `Services/TaskService.cs` - Add GetSortedAsync method | 10 min | 1.22 | Pending |
| 1.27 | Create `Services/TaskService.cs` - Add ClearCompletedAsync method | 10 min | 1.22 | Pending |
| 1.28 | Create `Services/TaskService.cs` - Add DuplicateAsync method | 10 min | 1.22 | Pending |
| 1.29 | Create `Services/TaskService.cs` - Add MoveToProjectAsync method | 10 min | 1.22 | Pending |
| 1.30 | Create `Services/TaskService.cs` - Add ToggleCompleteAsync method | 5 min | 1.22 | Pending |
| 1.31 | Create `Services/TaskService.cs` - Add GetFocusedAsync method | 5 min | 1.22 | Pending |
| 1.32 | Create `Services/TaskService.cs` - Add ToggleFocusAsync method | 5 min | 1.22 | Pending |
| 1.33 | Create `Services/KeyboardService.cs` | 15 min | 1.18 | Pending |
| 1.34 | Register services as scoped in `Program.cs` | 5 min | 1.20, 1.22, 1.33 | Pending |

**Phase 1C Checklist**:
- [ ] ProjectService with all CRUD operations
- [ ] TaskService with all CRUD operations
- [ ] TaskService has filter methods (priority, status, search)
- [ ] TaskService has sort methods
- [ ] TaskService has bulk operations (clear completed, duplicate, move)
- [ ] TaskService has focus methods
- [ ] KeyboardService with global shortcut handling
- [ ] All services registered as scoped

#### Phase 1D: UI Components - Sidebar & Projects (12 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 1.35 | Update `Layout/MainLayout.razor` with dark theme + sidebar layout | 20 min | 0.17 | Pending |
| 1.36 | Create `Presentation/Components/Sidebar/ProjectSidebar.razor` with project list | 25 min | 0.35, 1.29 | Pending |
| 1.37 | Add project color badges in sidebar items | 10 min | 1.36 | Pending |
| 1.38 | Add project icon support (MudBlazor icons) | 10 min | 1.36 | Pending |
| 1.39 | Add task count badge on each project item | 10 min | 1.36, 1.31 | Pending |
| 1.40 | Implement active project highlighting | 5 min | 1.36 | Pending |
| 1.41 | Add "Add Project" button in sidebar | 5 min | 1.36 | Pending |
| 1.42 | Make sidebar responsive (collapsible on mobile, 250px on desktop) | 15 min | 1.36 | Pending |
| 1.43 | Create `Presentation/Pages/Index.razor` for project management | 20 min | 1.35 | Pending |
| 1.44 | Implement project list with edit/delete in Index.razor | 15 min | 1.43, 1.29 | Pending |
| 1.45 | Implement add new project form | 15 min | 1.43, 1.29 | Pending |
| 1.46 | Implement project color picker | 10 min | 1.45 | Pending |
| 1.47 | Test project CRUD operations | 10 min | 1.44-1.46 | Pending |

**Phase 1D Checklist**:
- [ ] MainLayout with dark theme and sidebar
- [ ] ProjectSidebar with project list
- [ ] Project color badges displayed
- [ ] Project icons displayed
- [ ] Task count badges working
- [ ] Active project highlighted
- [ ] Add Project button functional
- [ ] Sidebar responsive (mobile hamburger, desktop 250px)
- [ ] Index page with project list
- [ ] Project edit/delete working
- [ ] Add project form functional
- [ ] Project color picker working

#### Phase 1E: UI Components - Tasks Page (20 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 1.48 | Create `Pages/Tasks.razor` with project tasks view | 20 min | 1.35, 1.22 | Pending |
| 1.49 | Add project header with name and stats | 10 min | 1.48, 1.21 | Pending |
| 1.50 | Implement search input with real-time filtering | 15 min | 1.48, 1.25 | Pending |
| 1.51 | Implement sort dropdown (Created date / Priority / Focused) | 15 min | 1.48, 1.26 | Pending |
| 1.52 | Implement show/hide completed toggle | 5 min | 1.48 | Pending |
| 1.53 | Implement filter by priority dropdown | 10 min | 1.48 | Pending |
| 1.54 | Implement "Clear All Completed" button | 10 min | 1.48, 1.27 | Pending |
| 1.55 | Implement add new task input | 10 min | 1.48, 1.22 | Pending |
| 1.56 | Create task list component in Todos.razor | 15 min | 1.48, 1.22 | Pending |
| 1.57 | Implement in-line editing for task titles | 20 min | 1.56, 1.22 | Pending |
| 1.58 | Implement priority dropdown with color coding | 10 min | 1.56 | Pending |
| 1.59 | Implement task note display (expandable) | 15 min | 1.56 | Pending |
| 1.60 | Implement complete checkbox | 5 min | 1.56, 1.30 | Pending |
| 1.61 | Implement delete task button | 5 min | 1.56, 1.22 | Pending |
| 1.62 | Implement duplicate task action | 10 min | 1.56, 1.28 | Pending |
| 1.63 | Implement move to project selector | 10 min | 1.56, 1.29 | Pending |
| 1.64 | Implement focus pin button | 5 min | 1.56, 1.32 | Pending |
| 1.65 | Add CreatedAt and CompletedAt timestamps | 10 min | 1.56 | Pending |
| 1.66 | Add visual indicator for recently completed tasks | 5 min | 1.56 | Pending |
| 1.67 | Implement empty state component | 10 min | 1.56 | Pending |

**Phase 1E Checklist**:
- [ ] Todos.razor created with task list
- [ ] Project header with name and stats
- [ ] Search input working with real-time filtering
- [ ] Sort dropdown working (Created date, Priority, Focused)
- [ ] Show/hide completed toggle working
- [ ] Filter by priority working
- [ ] "Clear All Completed" button working
- [ ] Add new task input functional
- [ ] Task list displaying tasks
- [ ] In-line editing for titles working
- [ ] Priority dropdown with color codes working
- [ ] Task note display and editing working
- [ ] Complete checkbox working
- [ ] Delete button working
- [ ] Duplicate task working
- [ ] Move to project working
- [ ] Focus pin working
- [ ] Timestamps displayed
- [ ] Empty state displayed

#### Phase 1F: Keyboard Shortcuts (8 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 1.68 | Implement keyboard shortcut handler service | 10 min | 1.33 | Pending |
| 1.69 | Add Ctrl/Cmd + Enter shortcut (complete selected task) | 5 min | 1.68, 1.30 | Pending |
| 1.70 | Add Delete shortcut (delete selected task) | 5 min | 1.68, 1.61 | Pending |
| 1.71 | Add Ctrl/Cmd + N shortcut (focus new task input) | 5 min | 1.68, 1.55 | Pending |
| 1.72 | Add Ctrl/Cmd + F shortcut (focus search input) | 5 min | 1.68, 1.50 | Pending |
| 1.73 | Add Ctrl/Cmd + P shortcut (toggle focus pin) | 5 min | 1.68, 1.64 | Pending |
| 1.74 | Add Ctrl/Cmd + Z shortcut (undo last action) | 10 min | 1.68 | Pending |
| 1.75 | Add Ctrl/Cmd + / shortcut (show keyboard shortcuts help) | 10 min | 1.68 | Pending |

**Phase 1F Checklist**:
- [ ] Keyboard shortcut handler implemented
- [ ] Ctrl/Cmd + Enter: Complete task
- [ ] Delete: Delete task
- [ ] Ctrl/Cmd + N: Focus new task
- [ ] Ctrl/Cmd + F: Focus search
- [ ] Ctrl/Cmd + P: Toggle focus pin
- [ ] Ctrl/Cmd + Z: Undo
- [ ] Ctrl/Cmd + /: Show help

#### Phase 1G: Toast Notifications (6 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 1.76 | Implement toast notification system | 15 min | 1.35 | Pending |
| 1.77 | Add success toast for completed tasks | 5 min | 1.76 | Pending |
| 1.78 | Add info toast for duplicated/moved tasks | 5 min | 1.76, 1.62, 1.63 | Pending |
| 1.79 | Add confirmation toast for deleted tasks | 5 min | 1.76, 1.61 | Pending |
| 1.80 | Add toast for "Clear All Completed" | 5 min | 1.76, 1.54 | Pending |
| 1.81 | Test all toast notifications and undo functionality | 10 min | 1.77-1.80 | Pending |

**Phase 1G Checklist**:
- [ ] Toast notification system implemented
- [ ] Success toasts showing
- [ ] Info toasts showing
- [ ] Confirmation toasts showing
- [ ] Undo buttons working
- [ ] Toast auto-dismissing after 5 seconds

#### Phase 1H: Docker Configuration (7 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 1.82 | Create Dockerfile for Blazor Server (ARM64) | 15 min | 1.7 | Pending |
| 1.83 | Create `Projects/taskflow-nginx/nginx.conf` reverse proxy | 10 min | - | Pending |
| 1.84 | Add `ts-taskflow` service to `/home/vip32/docker-compose.yaml` | 10 min | 1.1, 1.82 | Pending |
| 1.85 | Add `taskflow` service with volume and ports | 10 min | 1.82, 1.83 | Pending |
| 1.86 | Add `taskflow-nginx` service with Uptime Kuma labels | 10 min | 1.83, 1.85 | Pending |
| 1.87 | Review docker-compose configuration | 5 min | 1.84-1.86 | Pending |
| 1.88 | Verify network_mode pattern matches existing services | 5 min | 1.87 | Pending |

**Phase 1H Checklist**:
- [ ] Dockerfile created for ARM64
- [ ] nginx.conf created with reverse proxy
- [ ] ts-taskflow service added to docker-compose
- [ ] taskflow service added
- [ ] taskflow-nginx service added
- [ ] Uptime Kuma labels configured
- [ ] Network mode verified

#### Phase 1I: Build & Deploy (6 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 1.89 | Build Docker image: `docker-compose build taskflow` | 5 min | 1.82 | Pending |
| 1.90 | Test build completes without errors | 2 min | 1.89 | Pending |
| 1.91 | Start containers: `docker-compose up -d ts-taskflow taskflow taskflow-nginx` | 5 min | 1.90 | Pending |
| 1.92 | Verify containers are running | 2 min | 1.91 | Pending |
| 1.93 | Access at `http://taskflow.churra-platy.ts.net` | 2 min | 1.92 | Pending |
| 1.94 | Check Tailscale accessibility | 2 min | 1.93 | Pending |

**Phase 1I Checklist**:
- [ ] Docker image built successfully
- [ ] Build completed without errors
- [ ] Containers started
- [ ] All containers running
- [ ] App accessible via Tailscale
- [ ] App loads in browser

### Phase 1 Summary
- **Total Tasks**: 98
- **Total Estimated Time**: 3-4 hours
- **Dependencies**: All Phase 1A → 1B → 1C → 1D → 1E → 1F → 1G → 1H → 1I
- **Deliverable**: Fully functional TaskFlow application with core features

---

### Phase 2: Board Views + SubTasks (35 tasks)

#### Phase 2A: Database Schema Updates (6 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 2.1 | Update `Models/Project.cs` - Add `ViewType` property (enum: List/Board) | 5 min | Phase 1B | Pending |
| 2.2 | Update `Models/Task.cs` - Set `Status` to required (enum: ToDo/InProgress/Done) | 2 min | 1.10 | Pending |
| 2.3 | Update `Models/Task.cs` - Set `ParentTaskId` to required (nullable Guid) | 2 min | 1.10 | Pending |
| 2.4 | Update `Data/AppDbContext.cs` - Add navigation properties for SubTasks | 5 min | 2.2-2.3 | Pending |
| 2.5 | Add migration or update database creation logic for new fields | 10 min | 2.4 | Pending |
| 2.6 | Update seeding to include sample SubTasks and board view project | 5 min | 2.5 | Pending |

**Phase 2A Checklist**:
- [ ] Project model has ViewType property
- [ ] Task model has required Status
- [ ] Task model has required ParentTaskId
- [ ] DbContext has navigation properties
- [ ] Database migration/update applied
- [ ] Seeding updated with SubTasks

#### Phase 2B: Service Updates (8 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 2.7 | Update `Services/TaskService.cs` - Add GetByStatusAsync | 5 min | 2.2 | Pending |
| 2.8 | Update `Services/TaskService.cs` - Add GetSubTasksAsync | 5 min | 2.3 | Pending |
| 2.9 | Update `Services/TaskService.cs` - Add CreateSubTaskAsync | 10 min | 2.8 | Pending |
| 2.10 | Update `Services/TaskService.cs` - Add UpdateStatusAsync | 5 min | 2.2 | Pending |
| 2.11 | Update `Services/TaskService.cs` - Add ToggleCompleteWithSubTasksAsync | 10 min | 2.8, 2.10 | Pending |
| 2.12 | Update `Services/ProjectService.cs` - Add UpdateViewTypeAsync | 5 min | 2.1 | Pending |
| 2.13 | Update `Services/ProjectService.cs` - Update Get to include ViewType | 5 min | 2.1 | Pending |
| 2.14 | Test all new service methods | 10 min | 2.7-2.13 | Pending |

**Phase 2B Checklist**:
- [ ] GetByStatusAsync implemented
- [ ] GetSubTasksAsync implemented
- [ ] CreateSubTaskAsync implemented
- [ ] UpdateStatusAsync implemented
- [ ] ToggleCompleteWithSubTasksAsync implemented
- [ ] UpdateViewTypeAsync implemented
- [ ] Project Get includes ViewType
- [ ] All methods tested

#### Phase 2C: Components - Board View (10 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 2.15 | Create `Components/BoardView.razor` - Kanban board component | 20 min | 2.12 | Pending |
| 2.16 | Create `Components/BoardColumn.razor` - Individual column component | 15 min | 2.15 | Pending |
| 2.17 | Implement drag-and-drop for moving tasks between columns | 25 min | 2.16 | Pending |
| 2.18 | Add view switcher toggle (List ↔ Board) in Todos.razor | 10 min | 2.15 | Pending |
| 2.19 | Style board columns with proper spacing and colors | 10 min | 2.16 | Pending |
| 2.20 | Implement add task button on each board column | 10 min | 2.16 | Pending |
| 2.21 | Add visual indicators for tasks with SubTasks | 5 min | 2.16, 2.8 | Pending |
| 2.22 | Implement task count badges per column | 5 min | 2.16 | Pending |
| 2.23 | Ensure board view is responsive (stack columns on mobile) | 10 min | 2.16 | Pending |
| 2.24 | Test drag-and-drop interactions | 10 min | 2.17 | Pending |

**Phase 2C Checklist**:
- [ ] BoardView component created
- [ ] BoardColumn component created
- [ ] Drag-and-drop implemented between columns
- [ ] View switcher functional
- [ ] Board columns styled
- [ ] Add task button on each column
- [ ] Visual indicators for SubTasks
- [ ] Task count badges working
- [ ] Responsive design (stack on mobile)
- [ ] Drag-and-drop tested

#### Phase 2D: Components - SubTasks (8 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 2.25 | Create `Components/SubTaskList.razor` - SubTasks display component | 15 min | 2.8 | Pending |
| 2.26 | Implement in-line editing for SubTask titles | 10 min | 2.25 | Pending |
| 2.27 | Implement add SubTask input under parent Task | 10 min | 2.25 | Pending |
| 2.28 | Implement delete SubTask with toast + undo | 5 min | 2.25, 1.79 | Pending |
| 2.29 | Add SubTask count badge on parent Task cards | 5 min | 2.25, 2.8 | Pending |
| 2.30 | Implement expand/collapse SubTasks on parent Task | 10 min | 2.25 | Pending |
| 2.31 | Style SubTasks with indentation hierarchy | 5 min | 2.25 | Pending |
| 2.32 | Test auto-complete SubTasks when parent completes | 10 min | 2.11 | Pending |

**Phase 2D Checklist**:
- [ ] SubTaskList component created
- [ ] In-line editing for SubTasks
- [ ] Add SubTask input working
- [ ] Delete SubTask with toast + undo
- [ ] SubTask count badge on parent
- [ ] Expand/collapse SubTasks working
- [ ] Indentation styling applied
- [ ] Auto-complete SubTasks tested

#### Phase 2E: UI Integration (6 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 2.33 | Update `Pages/Todos.razor` - Integrate view switcher (List/Board) | 10 min | 2.18 | Pending |
| 2.34 | Update `Pages/Todos.razor` - Show SubTasks in List view | 15 min | 2.30 | Pending |
| 2.35 | Update `Pages/Index.razor` - Add view type selector per project | 10 min | 2.12 | Pending |
| 2.36 | Add status filter dropdown (ToDo/InProgress/Done/All) | 10 min | 2.7 | Pending |
| 2.37 | Update empty state for each board column | 5 min | 2.16 | Pending |
| 2.38 | Ensure all interactions work in both List and Board views | 15 min | 2.33, 2.34 | Pending |

**Phase 2E Checklist**:
- [ ] Todos.razor integrates view switcher
- [ ] SubTasks shown in List view
- [ ] Index has view type selector
- [ ] Status filter working
- [ ] Empty states updated for board columns
- [ ] All interactions tested in both views

#### Phase 2F: Testing & Verification (7 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 2.39 | Test switching between List and Board views | 5 min | 2.38 | Pending |
| 2.40 | Test moving tasks between statuses in Board view | 5 min | 2.24 | Pending |
| 2.41 | Test creating SubTasks on parent Tasks | 5 min | 2.27 | Pending |
| 2.42 | Test auto-completing SubTasks when parent completes | 5 min | 2.32 | Pending |
| 2.43 | Test adding tasks directly from board columns | 5 min | 2.20 | Pending |
| 2.44 | Test drag-and-drop with SubTasks | 5 min | 2.24 | Pending |
| 2.45 | Verify all new features work on mobile responsive design | 10 min | 2.23 | Pending |

**Phase 2F Checklist**:
- [ ] List/Board switching tested
- [ ] Task movement tested in Board view
- [ ] SubTask creation tested
- [ ] Auto-complete SubTasks tested
- [ ] Add to column tested
- [ ] Drag-and-drop with SubTasks tested
- [ ] Mobile responsiveness verified

### Phase 2 Summary
- **Total Tasks**: 35
- **Total Estimated Time**: 2-3 hours
- **Dependencies**: Phase 1 completed
- **Deliverable**: Functional Kanban board + SubTask support

---

### Phase 3: My Task Flow + Focus Timer + Import/Export (25 tasks)

#### Phase 3A: My Task Flow Views (10 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 3.1 | Update Task model to add IsTodayMarked property | 5 min | Phase 2A | Pending |
| 3.2 | Update `Application/Orchestrators/TaskOrchestrator.cs` - Add GetTodayTasksAsync | 10 min | 3.1 | Pending |
| 3.3 | Update `Application/Orchestrators/TaskOrchestrator.cs` - Add GetUpcomingTasksAsync | 10 min | 3.1 | Pending |
| 3.4 | Update `Application/Orchestrators/TaskOrchestrator.cs` - Add GetRecentTasksAsync | 10 min | 3.1 | Pending |
| 3.5 | Update `Application/Orchestrators/TaskOrchestrator.cs` - Add ToggleTodayMarkAsync | 5 min | 3.1 | Pending |
| 3.6 | Update ProjectSidebar to include My Task Flow section | 10 min | 1.36 | Pending |
| 3.7 | Create `Pages/Today.razor` view | 15 min | 3.2 | Pending |
| 3.8 | Create `Pages/Upcoming.razor` view | 15 min | 3.3 | Pending |
| 3.9 | Create `Pages/Recent.razor` view | 10 min | 3.4 | Pending |
| 3.10 | Test all My Task Flow views | 10 min | 3.7-3.9 | Pending |

**Phase 3A Checklist**:
- [ ] Task model has IsTodayMarked property
- [ ] GetTodayTasksAsync implemented
- [ ] GetUpcomingTasksAsync implemented
- [ ] GetRecentTasksAsync implemented
- [ ] ToggleTodayMarkAsync implemented
- [ ] Sidebar updated with My Task Flow section
- [ ] Today view created
- [ ] Upcoming view created
- [ ] Recent view created
- [ ] All views tested

#### Phase 3B: Focus Timer (10 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 3.11 | Create `Domain/Entities/FocusSession.cs` aggregate | 10 min | Phase 1B | Pending |
| 3.12 | Update `Infrastructure/Data/AppDbContext.cs` - Add FocusSession DbSet | 5 min | 3.11 | Pending |
| 3.13 | Create `Domain/Interfaces/IFocusSessionRepository.cs` | 5 min | 3.11 | Pending |
| 3.14 | Create `Infrastructure/Data/Repositories/FocusSessionRepository.cs` | 10 min | 3.13 | Pending |
| 3.15 | Create `Application/Orchestrators/FocusTimerOrchestrator.cs` | 15 min | 3.14 | Pending |
| 3.16 | Implement Pomodoro timer logic (25/5/15/5 cycles) | 15 min | 3.15 | Pending |
| 3.17 | Add timer configuration (custom durations, sounds) | 10 min | 3.15 | Pending |
| 3.18 | Implement sound notifications for timer events | 10 min | 3.15 | Pending |
| 3.19 | Create `Components/FocusTimer.razor` | 20 min | 3.16 | Pending |
| 3.20 | Test complete timer workflow | 15 min | 3.16-3.19 | Pending |

**Phase 3B Checklist**:
- [ ] FocusSession aggregate created
- [ ] DbContext includes FocusSession
- [ ] IFocusSessionRepository created
- [ ] FocusSessionRepository implemented
- [ ] FocusTimerOrchestrator created
- [ ] Pomodoro timer logic working
- [ ] Timer configuration functional
- [ ] Sound notifications working
- [ ] FocusTimer component created
- [ ] Complete workflow tested

#### Phase 3C: Import/Export (5 tasks)

| # | Task | Est. Time | Dependencies | Status |
|----|------|-----------|--------------|--------|
| 3.21 | Create JSON export DTOs for Project and Task | 10 min | Phase 2A | Pending |
| 3.22 | Add ExportProjectsAsync to ProjectOrchestrator | 15 min | 3.21 | Pending |
| 3.23 | Add ImportProjectsAsync to ProjectOrchestrator | 20 min | 3.21 | Pending |
| 3.24 | Create `Components/ProjectExport.razor` - Export dialog with project selector | 20 min | 3.22 | Pending |
| 3.25 | Create `Components/ProjectImport.razor` - Import dialog with file upload | 20 min | 3.23 | Pending |
| 3.26 | Test export/import workflow | 15 min | 3.24-3.25 | Pending |

**Phase 3C Checklist**:
- [ ] JSON export DTOs created (ProjectExportDto, TaskExportDto)
- [ ] ExportProjectsAsync implemented (returns JSON string)
- [ ] ImportProjectsAsync implemented (accepts JSON, uses IDs to add/update)
- [ ] ProjectExport component created with multi-select
- [ ] ProjectImport component created with file upload
- [ ] Export/import workflow tested
- [ ] ID-based add/update logic verified

### Phase 3 Summary
- **Total Tasks**: 26
- **Total Estimated Time**: 2-3 hours
- **Dependencies**: Phase 2 completed
- **Deliverable**: My Task Flow views + Focus timer + Import/Export

---

## Implementation Dependencies

### Phase Dependencies
```
Phase 1 (Core Features)
    ↓
Phase 2 (Board + SubTasks)
    ↓
Phase 3 (My Task Flow + Focus Timer)
```

### Task Dependencies Within Phase 1
```
1A (Project Setup)
    ↓
1B (Server Implementation)
    ↓
1C (Services Layer)
    ↓
1D (UI Components - Sidebar & Projects)
    ↓
1E (UI Components - Tasks Page)
    ↓
1F (Keyboard Shortcuts)
    ↓
1G (Toast Notifications)
    ↓
1H (Docker Configuration)
    ↓
1I (Build & Deploy)
```

### Task Dependencies Within Phase 2
```
2A (Database Schema Updates)
    ↓
2B (Service Updates)
    ↓
2C (Components - Board View)
    ↓
2D (Components - SubTasks)
    ↓
2E (UI Integration)
    ↓
2F (Testing & Verification)
```

### Task Dependencies Within Phase 3
```
3A (My Task Flow Views)
    ↓
3B (Focus Timer)
```

---

## Risk Assessment

### Technical Risks

| Risk | Impact | Probability | Mitigation |
|-------|---------|-------------|------------|
| Blazor Server performance issues on Raspberry Pi 5 | Medium | Low | Optimize queries, use AsNoTracking for read-only, implement virtualization |
| SQLite file corruption on power loss | Medium | Low | Regular backups, journal mode for SQLite, transaction safety |
| MudBlazor theme conflicts with custom Turquoise colors | Low | Low | Test theme thoroughly, use CSS custom properties to override defaults |
| Docker image build failures on ARM64 | Medium | Low | Test build locally first, use official .NET ARM64 SDK images |
| SignalR connection issues across Tailscale network | Medium | Medium | Implement graceful fallback, add connection status indicator |
| Drag-and-drop complexity in Board view | Low | Medium | Use tested MudBlazor drag-drop components, add visual feedback |

### Scope Risks

| Risk | Impact | Probability | Mitigation |
|-------|---------|-------------|------------|
| Feature creep adding more tasks | Medium | Medium | Stick to requirements, defer non-essential features to Phase 4 |
| Time estimation underestimation | Medium | Medium | Buffer 20% time, be prepared to defer Phase 3 if needed |
| Complexity higher than expected | Low | Low | Start with MVP, iterate, focus on core features first |

### Deployment Risks

| Risk | Impact | Probability | Mitigation |
|-------|---------|-------------|------------|
| Tailscale authentication issues | High | Low | Have backup TS_AUTHKEY ready, document TS setup process |
| Port conflicts with existing services | Medium | Low | Check existing docker-compose, use unique ports in internal network |
| Nginx configuration errors | Medium | Low | Test nginx.conf locally, verify with docker-compose up --dry-run |

---

## Testing Approach

### Unit Testing Stack
- **Framework**: xUnit
- **Pattern**: AAA (Arrange, Act, Assert)
- **Assertion Library**: Shouldly
- **Status**: Code structure supports testing, but test implementation deferred per requirements
- **Note**: All services and domain entities are testable

### Unit Testing Guidelines

#### Domain Entity Testing
```csharp
[Fact]
public void Complete_ShouldSetCompletedAndCompleteSubTasks()
{
    // Arrange
    var parentTask = new Task { Id = Guid.NewGuid(), Title = "Parent" };
    var subTask = new Task { Id = Guid.NewGuid(), Title = "Sub" };
    parentTask.AddSubTask(subTask);

    // Act
    parentTask.Complete();

    // Assert
    parentTask.IsCompleted.ShouldBeTrue();
    parentTask.CompletedAt.ShouldNotBeNull();
    parentTask.Status.ShouldBe(TaskStatus.Done);
    subTask.IsCompleted.ShouldBeTrue();
}
```

#### Orchestrator Testing
```csharp
[Fact]
public async Task CreateTaskAsync_ShouldAddTaskToProject()
{
    // Arrange
    var mockTaskRepo = new Mock<ITaskRepository>();
    var mockProjectRepo = new Mock<IProjectRepository>();
    var project = new Project { Id = Guid.NewGuid(), Name = "Test" };
    mockProjectRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(project);
    var orchestrator = new TaskOrchestrator(mockTaskRepo.Object, mockProjectRepo.Object);

    // Act
    var task = await orchestrator.CreateTaskAsync("Test Task", project.Id);

    // Assert
    task.ShouldNotBeNull();
    task.Title.ShouldBe("Test Task");
    mockTaskRepo.Verify(r => r.AddAsync(It.IsAny<Task>()), Times.Once);
}
```

#### Repository Testing (with In-Memory SQLite)
```csharp
[Fact]
public async Task AddAsync_ShouldAddTaskToDatabase()
{
    // Arrange
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;
    using var context = new AppDbContext(options);
    var repository = new TaskRepository(context);
    var task = new Task { Id = Guid.NewGuid(), Title = "Test" };

    // Act
    await repository.AddAsync(task);

    // Assert
    var addedTask = await context.Tasks.FindAsync(task.Id);
    addedTask.ShouldNotBeNull();
    addedTask.Title.ShouldBe("Test");
}
```

### Integration Testing

#### Phase 1 Testing
1. **Database Testing**
   - Create project and verify persistence
   - Create task and verify persistence
   - Test CRUD operations (create, read, update, delete)
   - Test database seeding on first startup
   - Test foreign key constraints

2. **Service Layer Testing**
   - Test all ProjectService methods
   - Test all TaskService methods
   - Test filter methods (priority, search, sort)
   - Test bulk operations (clear completed, duplicate, move)
   - Test focus methods

3. **UI Component Testing**
   - Test project creation/edit/delete
   - Test task creation/edit/delete
   - Test in-line editing
   - Test priority changes
   - Test note editing
   - Test focus pin toggle
   - Test search functionality
   - Test sort functionality
   - Test filter functionality
   - Test keyboard shortcuts
   - Test toast notifications and undo

4. **Responsive Design Testing**
   - Test on desktop (>1024px)
   - Test on tablet (768-1024px)
   - Test on mobile (<768px)
   - Test sidebar collapse/expand
   - Test board view stacking on mobile

#### Phase 2 Testing
1. **Board View Testing**
   - Test board view rendering
   - Test drag-and-drop between columns
   - Test view switcher (List ↔ Board)
   - Test add task to columns
   - Test task count badges
   - Test responsive stacking

2. **SubTask Testing**
   - Test SubTask creation
   - Test SubTask editing
   - Test SubTask deletion with undo
   - Test expand/collapse SubTasks
   - Test auto-complete SubTasks when parent completes
   - Test SubTask indentation

3. **Integration Testing**
   - Test SubTasks in both List and Board views
   - Test all CRUD operations with SubTasks
   - Test view state persistence

#### Phase 3 Testing
1. **My Task Flow Testing**
   - Test Today view (tasks due today + marked tasks)
   - Test Upcoming view (future tasks grouped by date)
   - Test Recent view (last 7 days)
   - Test view switching
   - Test task marking for today

2. **Focus Timer Testing**
   - Test timer start/pause/stop
   - Test Pomodoro cycles (25/5/15/5)
   - Test custom timer durations
   - Test sound notifications
   - Test timer history
   - Test timer with task association

### End-to-End Testing

#### User Journey Testing

**Scenario 1: Create and Complete Task**
1. Navigate to default Inbox project
2. Create new task with title "Test task"
3. Edit task to add note
4. Set priority to High
5. Mark task as focused
6. Complete task
7. Verify toast notification appears
8. Test undo and re-complete

**Scenario 2: Project Management**
1. Create new project "Test Project"
2. Select custom color and icon
3. Create tasks in project
4. Switch between projects
5. Edit project properties
6. Delete project and verify tasks removed

**Scenario 3: Search and Filter**
1. Create multiple tasks with different priorities
2. Use search to find specific task
3. Filter by priority (High)
4. Sort by Created date
5. Clear completed tasks
6. Verify all filters work together

**Scenario 4: Board View (Phase 2)**
1. Switch project to Board view
2. Create tasks in different statuses
3. Drag task from ToDo to InProgress
4. Drag task from InProgress to Done
5. Add task directly to column
6. Verify task counts update

**Scenario 5: SubTasks (Phase 2)**
1. Create parent task
2. Add 3 SubTasks
3. Complete SubTask 2
4. Expand/collapse SubTasks
5. Complete parent task
6. Verify all SubTasks auto-complete

**Scenario 6: My Task Flow (Phase 3)**
1. Navigate to Today view
2. Mark task for today
3. Switch to Upcoming view
4. Navigate to Recent view
5. Return to project view
6. Verify view state persists

**Scenario 7: Focus Timer (Phase 3)**
1. Select task
2. Start focus timer
3. Complete 25-minute work session
4. Take 5-minute break
6. Complete work session
7. View timer history
8. Verify all timer events trigger

---

## Deployment Plan

### Pre-Deployment Checklist
- [ ] All requirements reviewed and understood
- [ ] SYSTEMDESIGN.md reviewed
- [ ] REQUIREMENTS.md reviewed
- [ ] PLANNING.md reviewed
- [ ] Development environment ready (.NET 10.0 SDK, Docker)
- [ ] Tailscale TS_AUTHKEY available
- [ ] docker-compose.yaml backup created
- [ ] Previous containers stopped (if any)

### Deployment Steps

#### Step 1: Project Initialization
```bash
# Create directories
mkdir -p /home/vip32/Projects/ts-taskflow/state
mkdir -p /home/vip32/Projects/taskflow-nginx

# Navigate to Projects directory
cd /home/vip32/Projects

# Generate Blazor Server project
dotnet new blazorserver -o taskflow
```

#### Step 2: Add Dependencies
```bash
cd /home/vip32/Projects/taskflow

# Add MudBlazor
dotnet add package MudBlazor

# Add EF Core SQLite
dotnet add package Microsoft.EntityFrameworkCore.Sqlite

# Add EF Core Design
dotnet add package Microsoft.EntityFrameworkCore.Design
```

#### Step 3: Database Configuration
```bash
# Update appsettings.json with SQLite connection string
# Connection: Data Source=/data/taskflow.db
```

#### Step 4: Build and Test Locally
```bash
# Build project
dotnet build

# Run project locally (test without Docker)
dotnet run

# Test database creation
# Test CRUD operations
# Test UI interactions
```

#### Step 5: Docker Image Build
```bash
# Build Docker image
docker-compose build taskflow

# Verify image was created
docker images | grep taskflow
```

#### Step 6: Container Deployment
```bash
# Start all three containers
docker-compose up -d ts-taskflow taskflow taskflow-nginx

# Verify all containers running
docker-compose ps ts-taskflow taskflow taskflow-nginx

# Check logs for any errors
docker-compose logs taskflow
```

#### Step 7: Tailscale Verification
```bash
# Check Tailscale connectivity
# Verify ts-taskflow container is connected
docker-compose logs ts-taskflow

# Test access from other Tailscale device
# Open http://taskflow.churra-platy.ts.net in browser
```

#### Step 8: Post-Deployment Verification
```bash
# Test all CRUD operations
# Test all UI interactions
# Test responsive design
# Test keyboard shortcuts
# Test toast notifications
# Test undo functionality

# Verify database persistence
docker-compose exec taskflow ls -la /data

# Create first backup
mkdir -p /home/vip32/backups/taskflow
docker cp taskflow:/data/taskflow.db /home/vip32/backups/taskflow/taskflow_initial.db
```

### Deployment Verification

**Health Checks:**
- [ ] All 3 containers running
- [ ] No errors in logs
- [ ] Database file created in volume
- [ ] App loads in browser
- [ ] Tailscale connection successful
- [ ] All CRUD operations working
- [ ] All UI interactions working
- [ ] Responsive design verified
- [ ] Keyboard shortcuts working
- [ ] Toast notifications working

**Performance Verification:**
- [ ] Page load time < 2 seconds
- [ ] Task CRUD operations < 200ms
- [ ] Search across 1000 tasks < 500ms
- [ ] UI updates via SignalR < 100ms

**Integration Verification:**
- [ ] Uptime Kuma detects service
- [ ] Nginx reverse proxy working
- [ ] SignalR connections stable
- [ ] Real-time updates working across multiple tabs

---

## Rollback Plan

### Rollback Triggers
- Critical bugs preventing basic functionality
- Data corruption issues
- Performance issues making app unusable
- Tailscale connectivity issues

### Rollback Procedures

#### Level 1: Configuration Rollback
```bash
# Rollback docker-compose.yaml changes
git checkout docker-compose.yaml

# Restart with previous configuration
docker-compose down
docker-compose up -d ts-taskflow taskflow taskflow-nginx
```

#### Level 2: Code Rollback
```bash
# Rollback to previous working commit
git log --oneline -10
git checkout <commit-hash>

# Rebuild and redeploy
docker-compose build taskflow
docker-compose up -d ts-taskflow taskflow taskflow-nginx
```

#### Level 3: Database Rollback
```bash
# Stop containers
docker-compose down ts-taskflow taskflow taskflow-nginx

# Restore database from backup
docker cp /home/vip32/backups/taskflow/taskflow_latest.db taskflow:/data/taskflow.db

# Restart containers
docker-compose up -d ts-taskflow taskflow taskflow-nginx
```

#### Level 4: Full Environment Rollback
```bash
# Stop all TaskFlow containers
docker-compose down ts-taskflow taskflow taskflow-nginx

# Remove images
docker-compose down --rmi all

# Remove volumes (WARNING: Data loss)
docker volume rm taskflow-data

# Start previous version (if any)
# Or rebuild from scratch using rollback procedures
```

### Rollback Verification
- [ ] Application loads successfully
- [ ] Database integrity verified
- [ ] No data loss (except intentional rollback)
- [ ] All basic features working
- [ ] Performance acceptable

---

## Success Criteria

### Phase 1 Success Criteria
- [ ] All project CRUD operations working
- [ ] All task CRUD operations working
- [ ] Task notes functional
- [ ] Task focus pins working
- [ ] Search, sort, filter all functional
- [ ] All keyboard shortcuts working
- [ ] Toast notifications working with undo
- [ ] Dark theme with Turquoise accent applied
- [ ] Fully responsive design
- [ ] Docker containers deployed and accessible
- [ ] No critical bugs remaining

### Phase 2 Success Criteria
- [ ] Board view functional with 3 columns
- [ ] Drag-and-drop working between columns
- [ ] View switcher working (List ↔ Board)
- [ ] SubTask CRUD operations working
- [ ] SubTasks display correctly indented
- [ ] Auto-complete SubTasks working
- [ ] All features working in both List and Board views
- [ ] No critical bugs remaining

### Phase 3 Success Criteria
- [ ] My Task Flow views functional (Today, Upcoming, Recent)
- [ ] Task marking for today working
- [ ] Focus timer functional
- [ ] Pomodoro cycles working
- [ ] Sound notifications working
- [ ] Timer history tracking working
- [ ] All My Task Flow views aggregating tasks correctly
- [ ] No critical bugs remaining

---

## Final Checklist

### Pre-Implementation
- [ ] All documentation reviewed
- [ ] Development environment verified
- [ ] Dependencies checked (SDK, Docker, Tailscale)
- [ ] Backup plan documented
- [ ] Rollback plan understood

### During Implementation
- [ ] Tasks completed in order
- [ ] Dependencies respected
- [ ] Code following best practices
- [ ] Regular builds tested
- [ ] Integration tests performed

### Post-Implementation
- [ ] All phases completed
- [ ] All features tested
- [ ] User acceptance criteria met
- [ ] Performance benchmarks met
- [ ] Documentation updated
- [ ] Backup scheduled
- [ ] Deployment verified

---

## Appendix

### A. Command Reference

#### .NET CLI Commands
```bash
# Create project
dotnet new blazorserver -o taskflow

# Add package
dotnet add package <package-name>

# Build project
dotnet build

# Run project
dotnet run

# Publish project
dotnet publish -c Release -o ./publish
```

#### Docker Commands
```bash
# Build image
docker-compose build taskflow

# Start containers
docker-compose up -d ts-taskflow taskflow taskflow-nginx

# Stop containers
docker-compose down ts-taskflow taskflow taskflow-nginx

# View logs
docker-compose logs taskflow
docker-compose logs -f ts-taskflow

# Execute command in container
docker-compose exec taskflow <command>

# Copy file from container
docker cp taskflow:/data/taskflow.db ./backup.db
```

#### Tailscale Commands
```bash
# View Tailscale status in container
docker-compose exec ts-taskflow tailscale status

# View Tailscale IP
docker-compose exec ts-taskflow tailscale ip -4
```

### B. File Locations

```
/home/vip32/Projects/
├── taskflow/                # Main application
│   ├── TaskFlow.csproj
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Data/
│   ├── Models/
│   ├── Services/
│   ├── Components/
│   ├── Layout/
│   ├── Pages/
│   ├── wwwroot/
│   └── Dockerfile
├── ts-taskflow/
│   └── state/              # Tailscale state
├── taskflow-nginx/
│   └── nginx.conf          # Nginx config
└── backups/
    └── taskflow/          # Database backups
```

### C. Time Tracking

| Phase | Estimated Time | Actual Time | Variance | Notes |
|--------|---------------|-------------|-----------|-------|
| Phase 1 | 3-4 hours | ___ | ___ | |
| Phase 2 | 2-3 hours | ___ | ___ | |
| Phase 3 | 1-2 hours | ___ | ___ | |
| **Total** | **6-9 hours** | **___** | **___** | |

### D. Task Status Key
- **Pending**: Task not started
- **In Progress**: Task currently being worked on
- **Blocked**: Task blocked by dependency
- **Completed**: Task finished successfully
- **Deferred**: Task moved to future phase

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 2.0 | 2025-02-09 | System | Added Phase 0 (Clean Architecture Setup - 53 tasks), updated file structure for 4-layer onion, updated all phases to use application services, added component reusability tasks |
| 1.0 | 2025-02-09 | System | Initial planning document |

---

*Document Last Updated: February 9, 2026*
