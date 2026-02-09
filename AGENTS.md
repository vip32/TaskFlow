# TaskFlow - Implementation Agent Instructions

## Project Overview

TaskFlow is a personal task management web application built with **Blazor Server**, implementing **Domain-Driven Design (DDD)** with a **rich domain model** approach.

### Key Architectural Principles

1. **Rich Domain Model**: Business logic resides in domain entities, not in services
2. **No DTOs**: Domain aggregates are used directly in presentation and application layers
3. **Minimal Application Services**: Only orchestration, coordination, and transaction management
4. **No Business Services**: All business logic is in domain entities
5. **Repository Pattern**: Data access abstracted through interfaces
6. **Blazor Server**: Server-side rendering with SignalR for real-time updates

### Technology Stack

- **Framework**: .NET 10.0 (ASP.NET Core)
- **Runtime**: Blazor Server
- **Database**: SQLite with Entity Framework Core
- **UI Framework**: MudBlazor 7.x
- **Deployment**: Docker containers on Raspberry Pi 5 (ARM64)
- **Network**: Tailscale VPN for private access

---

## Architecture Overview

### 4-Layer Clean Architecture

```
┌─────────────────────────────────────────┐
│    Presentation Layer (Blazor)       │
│  - Razor Components & Pages           │
│  - UI logic only, no business logic │
│  - Uses domain aggregates directly   │
│  - @inject Orchestrators            │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Application Layer (Orchestration)   │
│  - Orchestrators (minimal)          │
│  - Transaction management            │
│  - Cross-aggregate coordination     │
│  - @inject Repositories            │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│      Domain Layer (Core)            │
│  - Rich entities with behavior      │
│  - Value objects (immutable)       │
│  - Repository interfaces           │
│  - No external dependencies       │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│  Infrastructure Layer (Data)        │
│  - EF Core DbContext               │
│  - Repository implementations      │
│  - Database seeding               │
└───────────────────────────────────┘
```

---

## Critical Rules

### ✅ DO

1. **Put business logic in domain entities**
   - Task.Complete() - marks task as complete
   - Task.AddSubTask() - adds child task
   - Task.MoveToProject() - changes project
   - Task.SetPriority() - changes priority
   - Task.ToggleFocus() - toggles focus pin
   - Project.AddTask() - adds task to project
   - Project.RemoveTask() - removes task from project
   - Project.UpdateViewType() - changes view type

2. **Use domain aggregates directly in presentation**
   - Components receive Task, Project entities
   - No mapping to DTOs
   - No DTO classes
   - Domain entities traverse all layers

 3. **Application services/orchestrators are allowed and encouraged**
    - Use repositories to coordinate domain operations
    - Handle transactions
    - Coordinate cross-aggregate operations
    - Delegate business logic to domain entities
    - Minimal code - just orchestration

 4. **Use repository pattern**
    - Repository interfaces in domain layer
    - Implementations in infrastructure layer
    - Only repositories work with DbContext
    - Orchestrators depend on interfaces only

5. **Follow naming conventions**
   - Async methods end with `Async`
   - Interfaces start with `I`
   - Domain entities: PascalCase
   - Methods: PascalCase
   - No Hungarian notation

### ❌ DO NOT

 1. **Do NOT create DTOs (except for import/export)**
    - No TaskDto, ProjectDto for general use
    - No mapping between entities and DTOs for presentation
    - Domain aggregates travel to presentation
    - Exception: JSON export/import DTOs for Phase 3 import/export feature

 2. **Do NOT put business logic in services**
    - Business logic belongs in domain entities
    - Application services/orchestrators only orchestrate between repositories
    - Minimal code in orchestrators - no business rules

 3. **Do NOT put DbContext access outside repositories**
    - No EF Core in orchestrators
    - No EF Core in presentation layer
    - Only repositories work with DbContext
    - Data access only in infrastructure layer

 4. **Do NOT violate dependency inversion**
    - Domain layer has no dependencies
    - Infrastructure depends on domain interfaces
    - Application depends on domain interfaces
    - Presentation depends on application interfaces

 5. **Do NOT create anemic domain model**
    - Entities should have behavior methods
    - Not just getters and setters
    - Rich domain = entities with logic

---

## Domain Model Design

### Task Aggregate

```csharp
// Domain/Task.cs
public class Task
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string? Note { get; private set; }
    public TaskPriority Priority { get; private set; }
    public bool IsCompleted { get; private set; }
    public bool IsFocused { get; private set; }
    public TaskStatus Status { get; private set; }
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } // Navigation
    public Guid? ParentTaskId { get; private set; }
    public Task ParentTask { get; private set; } // Navigation
    public List<Task> SubTasks { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    // Business logic methods
    public void Complete()
    {
        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
        Status = TaskStatus.Done;
        foreach (var subTask in SubTasks)
        {
            subTask.Complete();
        }
    }

    public void Uncomplete()
    {
        IsCompleted = false;
        CompletedAt = null;
        // Note: SubTasks stay completed per requirements
    }

    public void AddSubTask(Task subTask)
    {
        subTask.ParentTaskId = Id;
        SubTasks.Add(subTask);
    }

    public void SetPriority(TaskPriority priority)
    {
        Priority = priority;
    }

    public void ToggleFocus()
    {
        IsFocused = !IsFocused;
    }

    public void MoveToProject(Guid newProjectId)
    {
        ProjectId = newProjectId;
    }

    public void UpdateTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Title cannot be empty");
        Title = newTitle;
    }

    public void UpdateNote(string? newNote)
    {
        Note = newNote;
    }

    public void SetStatus(TaskStatus status)
    {
        Status = status;
        if (status == TaskStatus.Done)
        {
            Complete();
        }
    }
}
```

### Project Aggregate

```csharp
// Domain/Project.cs
public class Project
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Color { get; private set; }
    public string Icon { get; private set; }
    public bool IsDefault { get; private set; }
    public ProjectViewType ViewType { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public List<Task> Tasks { get; private set; } = new();

    // Business logic methods
    public void AddTask(Task task)
    {
        task.MoveToProject(Id);
        Tasks.Add(task);
    }

    public void RemoveTask(Task task)
    {
        Tasks.Remove(task);
    }

    public void UpdateViewType(ProjectViewType viewType)
    {
        ViewType = viewType;
    }

    public int GetTaskCount()
    {
        return Tasks.Count(t => !t.IsCompleted);
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be empty");
        Name = newName;
    }

    public void UpdateColor(string newColor)
    {
        Color = newColor;
    }

    public void UpdateIcon(string newIcon)
    {
        Icon = newIcon;
    }
}
```

### Value Objects

```csharp
// Domain/TaskPriority.cs
public enum TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3
}

// Domain/TaskStatus.cs
public enum TaskStatus
{
    ToDo,
    InProgress,
    Done
}

// Domain/ProjectViewType.cs
public enum ProjectViewType
{
    List,
    Board
}
```

---

## Application Layer (Orchestrators)

### Purpose

Orchestrators **only**:
- Coordinate between repositories
- Manage transactions
- Handle cross-aggregate operations
- Return domain aggregates to presentation

**DO NOT** put business logic here!

### TaskOrchestrator Example

```csharp
// Application/TaskOrchestrator.cs
public class TaskOrchestrator : ITaskOrchestrator
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;

    public TaskOrchestrator(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
    }

    public async Task<Task> CreateTaskAsync(string title, Guid projectId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            throw new NotFoundException("Project not found");

        var task = new Task
        {
            Id = Guid.NewGuid(),
            Title = title,
            Priority = TaskPriority.Medium,
            Status = TaskStatus.ToDo,
            IsCompleted = false,
            IsFocused = false,
            ProjectId = projectId,
            CreatedAt = DateTime.UtcNow
        };

        // Domain logic
        project.AddTask(task);

        await _taskRepository.AddAsync(task);
        return task;
    }

    public async Task<bool> CompleteTaskAsync(Guid taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            return false;

        // Domain logic - business rule here!
        task.Complete();

        await _taskRepository.UpdateAsync(task);
        return true;
    }

    public async Task<List<Task>> GetTasksByProjectAsync(Guid projectId)
    {
        // Just query - no business logic
        return await _taskRepository.GetByProjectIdAsync(projectId);
    }

    public async Task<bool> MoveTaskBetweenProjectsAsync(Guid taskId, Guid newProjectId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        var oldProject = await _projectRepository.GetByIdAsync(task.ProjectId);
        var newProject = await _projectRepository.GetByIdAsync(newProjectId);

        if (task == null || newProject == null)
            return false;

        // Cross-aggregate coordination
        oldProject?.RemoveTask(task);
        newProject.AddTask(task);

        // Domain logic
        task.MoveToProject(newProjectId);

        await _taskRepository.UpdateAsync(task);
        return true;
    }
}
```

---

## Presentation Layer

### Key Principles

1. **Inject orchestrators, not repositories**
2. **Use domain entities directly**
3. **Call domain methods for business operations**

### Component Example

```razor
@* Components/TaskItem.razor *@
@inject ITaskOrchestrator TaskOrchestrator

<div class="task-item">
    <MudCheckBox @bind-Checked="@task.IsCompleted"
                  Disabled="@task.IsCompleted"
                  OnClick="OnComplete" />

    <MudIconButton Icon="@task.IsFocused ? Icons.Material.Filled.PushPin : Icons.Material.Outlined.PushPin"
                   Color="@GetFocusColor()"
                   OnClick="OnToggleFocus" />

    @if (isEditingTitle)
    {
        <MudTextField @bind-Value="@editTitle"
                     OnBlur="SaveTitle"
                     OnEnter="SaveTitle"
                     Variant="Variant.Outlined" />
    }
    else
    {
        <span @onclick="StartEditTitle">@task.Title</span>
    }

    <PriorityBadge Priority="@task.Priority" />

    <MudIconButton Icon="Icons.Material.Filled.Delete"
                   Color="Color.Error"
                   OnClick="OnDelete" />
</div>

@code {
    [Parameter]
    public Task task { get; set; } = null!;

    private bool isEditingTitle = false;
    private string editTitle = string.Empty;

    private void OnComplete()
    {
        // Call orchestrator, which delegates to domain
        TaskOrchestrator.CompleteTaskAsync(task.Id);
    }

    private void OnToggleFocus()
    {
        // Domain logic in Task.ToggleFocus()
        task.ToggleFocus();
        TaskOrchestrator.UpdateTaskAsync(task);
    }

    private void StartEditTitle()
    {
        isEditingTitle = true;
        editTitle = task.Title;
    }

    private void SaveTitle()
    {
        // Domain logic in Task.UpdateTitle()
        task.UpdateTitle(editTitle);
        TaskOrchestrator.UpdateTaskAsync(task);
        isEditingTitle = false;
    }

    private void OnDelete()
    {
        TaskOrchestrator.DeleteTaskAsync(task.Id);
    }

    private Color GetFocusColor()
    {
        return task.IsFocused ? Color.Primary : Color.Default;
    }
}
```

---

## Infrastructure Layer

### Repository Implementation

```csharp
// Infrastructure/TaskRepository.cs
public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Task>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.SubTasks)
            .OrderByDescending(t => t.IsFocused)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Task?> GetByIdAsync(Guid id)
    {
        return await _context.Tasks
            .Include(t => t.SubTasks)
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Task> AddAsync(Task task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<Task> UpdateAsync(Task task)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var task = await GetByIdAsync(id);
        if (task == null)
            return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Task>> GetByPriorityAsync(TaskPriority priority, Guid projectId)
    {
        return await _context.Tasks
            .Where(t => t.ProjectId == projectId && t.Priority == priority)
            .ToListAsync();
    }

    public async Task<List<Task>> SearchAsync(string query, Guid projectId)
    {
        return await _context.Tasks
            .Where(t => t.ProjectId == projectId && t.Title.Contains(query))
            .ToListAsync();
    }

    public async Task<List<Task>> GetFocusedAsync(Guid projectId)
    {
        return await _context.Tasks
            .Where(t => t.ProjectId == projectId && t.IsFocused)
            .ToListAsync();
    }
}
```

### DbContext

```csharp
// Infrastructure/AppDbContext.cs
public class AppDbContext : DbContext
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<Task> Tasks { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Task configuration
        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(500);
            entity.Property(t => t.Note).HasMaxLength(2000);
            entity.HasOne(t => t.Project)
                  .WithMany(p => p.Tasks)
                  .HasForeignKey(t => t.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(t => t.ParentTask)
                  .WithMany(t => t.SubTasks)
                  .HasForeignKey(t => t.ParentTaskId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Project configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Color).IsRequired().HasMaxLength(7);
            entity.Property(p => p.Icon).IsRequired().HasMaxLength(50);
        });

        // Seeding
        modelBuilder.Entity<Project>().HasData(new Project
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = "Inbox",
            Color = "#40E0D0",
            Icon = "inbox",
            IsDefault = true,
            ViewType = ProjectViewType.List,
            CreatedAt = DateTime.UtcNow
        });
    }
}
```

---

## Import/Export Feature (Phase 3)

### Purpose
Allow users to export selected projects to JSON and import projects back, using IDs to determine whether to add new or update existing.

### Architecture

```
UI Components (ProjectExport, ProjectImport)
    ↓ @inject
ProjectOrchestrator
    ↓ @inject
ProjectRepository + TaskRepository
```

### Export DTOs

```csharp
// Application/DTOs/ProjectExportDto.cs
public class ProjectExportDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Color { get; set; }
    public string Icon { get; set; }
    public bool IsDefault { get; set; }
    public string ViewType { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TaskExportDto> Tasks { get; set; } = new();
}

// Application/DTOs/TaskExportDto.cs
public class TaskExportDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string? Note { get; set; }
    public string Priority { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsFocused { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<TaskExportDto> SubTasks { get; set; } = new();
}
```

### Export Implementation

```csharp
// Application/Orchestrators/ProjectOrchestrator.cs
public async Task<string> ExportProjectsAsync(List<Guid> projectIds)
{
    var projects = new List<ProjectExportDto>();

    foreach (var projectId in projectIds)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null) continue;

        // Map to DTO for JSON export
        var projectDto = new ProjectExportDto
        {
            Id = project.Id,
            Name = project.Name,
            Color = project.Color,
            Icon = project.Icon,
            IsDefault = project.IsDefault,
            ViewType = project.ViewType.ToString(),
            CreatedAt = project.CreatedAt
        };

        // Get all tasks for project
        var tasks = await _taskRepository.GetByProjectIdAsync(projectId);
        foreach (var task in tasks)
        {
            projectDto.Tasks.Add(MapTaskToDto(task));
        }

        projects.Add(projectDto);
    }

    return JsonSerializer.Serialize(projects, new JsonSerializerOptions
    {
        WriteIndented = true
    });
}

private TaskExportDto MapTaskToDto(Task task, bool includeSubTasks = true)
{
    return new TaskExportDto
    {
        Id = task.Id,
        Title = task.Title,
        Note = task.Note,
        Priority = task.Priority.ToString(),
        IsCompleted = task.IsCompleted,
        IsFocused = task.IsFocused,
        Status = task.Status.ToString(),
        CreatedAt = task.CreatedAt,
        CompletedAt = task.CompletedAt,
        SubTasks = includeSubTasks ? task.SubTasks.Select(st => MapTaskToDto(st, false)).ToList() : new()
    };
}
```

### Import Implementation

```csharp
// Application/Orchestrators/ProjectOrchestrator.cs
public async Task<int> ImportProjectsAsync(string jsonData)
{
    var projectsDto = JsonSerializer.Deserialize<List<ProjectExportDto>>(jsonData);
    if (projectsDto == null) return 0;

    int importedCount = 0;

    foreach (var projectDto in projectsDto)
    {
        var existingProject = await _projectRepository.GetByIdAsync(projectDto.Id);

        if (existingProject != null)
        {
            // Update existing project
            existingProject.UpdateName(projectDto.Name);
            existingProject.UpdateColor(projectDto.Color);
            existingProject.UpdateIcon(projectDto.Icon);
            existingProject.UpdateViewType(Enum.Parse<ProjectViewType>(projectDto.ViewType));

            await _projectRepository.UpdateAsync(existingProject);
        }
        else
        {
            // Create new project
            var newProject = new Project
            {
                Id = projectDto.Id,
                Name = projectDto.Name,
                Color = projectDto.Color,
                Icon = projectDto.Icon,
                IsDefault = projectDto.IsDefault,
                ViewType = Enum.Parse<ProjectViewType>(projectDto.ViewType),
                CreatedAt = projectDto.CreatedAt
            };

            newProject = await _projectRepository.AddAsync(newProject);
        }

        // Import tasks
        foreach (var taskDto in projectDto.Tasks)
        {
            await ImportTaskAsync(taskDto, projectDto.Id);
        }

        importedCount++;
    }

    return importedCount;
}

private async Task ImportTaskAsync(TaskExportDto taskDto, Guid projectId)
{
    var existingTask = await _taskRepository.GetByIdAsync(taskDto.Id);

    if (existingTask != null)
    {
        // Update existing task
        existingTask.UpdateTitle(taskDto.Title);
        existingTask.UpdateNote(taskDto.Note);
        existingTask.SetPriority(Enum.Parse<TaskPriority>(taskDto.Priority));
        existingTask.SetStatus(Enum.Parse<TaskStatus>(taskDto.Status));
        existingTask.MoveToProject(projectId);

        if (existingTask.IsCompleted != taskDto.IsCompleted)
        {
            if (taskDto.IsCompleted)
                existingTask.Complete();
            else
                existingTask.Uncomplete();
        }

        await _taskRepository.UpdateAsync(existingTask);
    }
    else
    {
        // Create new task
        var newTask = new Task
        {
            Id = taskDto.Id,
            Title = taskDto.Title,
            Note = taskDto.Note,
            Priority = Enum.Parse<TaskPriority>(taskDto.Priority),
            IsCompleted = taskDto.IsCompleted,
            IsFocused = taskDto.IsFocused,
            Status = Enum.Parse<TaskStatus>(taskDto.Status),
            ProjectId = projectId,
            CreatedAt = taskDto.CreatedAt,
            CompletedAt = taskDto.CompletedAt
        };

        if (taskDto.IsCompleted)
            newTask.Complete();

        newTask = await _taskRepository.AddAsync(newTask);
    }

    // Import subtasks
    foreach (var subTaskDto in taskDto.SubTasks)
    {
        await ImportSubTaskAsync(subTaskDto, taskDto.Id);
    }
}

private async Task ImportSubTaskAsync(TaskExportDto subTaskDto, Guid parentTaskId)
{
    var existingSubTask = await _taskRepository.GetByIdAsync(subTaskDto.Id);

    if (existingSubTask != null)
    {
        // Update existing subtask
        existingSubTask.UpdateTitle(subTaskDto.Title);
        existingSubTask.UpdateNote(subTaskDto.Note);
        existingSubTask.SetPriority(Enum.Parse<TaskPriority>(subTaskDto.Priority));
        existingSubTask.SetStatus(Enum.Parse<TaskStatus>(subTaskDto.Status));
        existingSubTask.ParentTaskId = parentTaskId;

        if (existingSubTask.IsCompleted != subTaskDto.IsCompleted)
        {
            if (subTaskDto.IsCompleted)
                existingSubTask.Complete();
        }

        await _taskRepository.UpdateAsync(existingSubTask);
    }
    else
    {
        // Create new subtask
        var newSubTask = new Task
        {
            Id = subTaskDto.Id,
            Title = subTaskDto.Title,
            Note = subTaskDto.Note,
            Priority = Enum.Parse<TaskPriority>(subTaskDto.Priority),
            IsCompleted = subTaskDto.IsCompleted,
            Status = Enum.Parse<TaskStatus>(subTaskDto.Status),
            ProjectId = null, // Will be set by parent
            ParentTaskId = parentTaskId,
            CreatedAt = subTaskDto.CreatedAt,
            CompletedAt = subTaskDto.CompletedAt
        };

        if (subTaskDto.IsCompleted)
            newSubTask.Complete();

        await _taskRepository.AddAsync(newSubTask);
    }
}
```

### UI Components

#### ProjectExport Component

```razor
@* Components/ProjectExport.razor *@
@inject IProjectOrchestrator ProjectOrchestrator
@inject ISnackbar Snackbar

<MudDialog>
    <DialogContent>
        <MudText Typo="Typo.h6">Export Projects</MudText>
        <MudText>Select projects to export:</MudText>
        <MudCheckBoxList @bind-SelectedValues="@selectedProjectIds" T="Guid">
            @foreach (var project in allProjects)
            {
                <MudCheckBoxItem Value="@project.Id">
                    @project.Name
                </MudCheckBoxItem>
            }
        </MudCheckBoxList>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        <MudButton Color="Color.Primary" OnClick="Export" Disabled="@selectedProjectIds.Count == 0">
            Export (@selectedProjectIds.Count projects)
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; }

    private List<Guid> selectedProjectIds = new();
    private List<Project> allProjects = new();

    protected override async Task OnInitializedAsync()
    {
        allProjects = await ProjectOrchestrator.GetAllProjectsAsync();
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private async Task Export()
    {
        try
        {
            var json = await ProjectOrchestrator.ExportProjectsAsync(selectedProjectIds);
            var blob = new byte[] { };
            // Use MudBlazor's file download or JS interop
            // For simplicity, show in textarea to copy
            await MudDialog.CloseAsync(DialogResult.Ok(json));
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Export failed: {ex.Message}", Severity.Error);
        }
    }
}
```

#### ProjectImport Component

```razor
@* Components/ProjectImport.razor *@
@inject IProjectOrchestrator ProjectOrchestrator
@inject ISnackbar Snackbar

<MudDialog>
    <DialogContent>
        <MudText Typo="Typo.h6">Import Projects</MudText>
        <MudText>Upload JSON file:</MudText>
        <MudFileInput T="string" OnChange="OnFileSelected" Label="Select JSON file" Accept=".json" />
        @if (!string.IsNullOrEmpty(importedData))
        {
            <MudTextField T="string" @bind-Value="@importedData"
                         Label="Or paste JSON here"
                         Multiline="true"
                         Rows="10" />
        }
        <MudText Class="mt-4">
            Projects with existing IDs will be updated. New IDs will create new projects.
        </MudText>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        <MudButton Color="Color.Primary" OnClick="Import" Disabled="@string.IsNullOrEmpty(importedData)">
            Import
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; }

    private string importedData = string.Empty;

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private void OnFileSelected(IBrowserFile file)
    {
        // Read file and populate importedData
        // (Use JS interop or MudBlazor's built-in file handling)
    }

    private async Task Import()
    {
        try
        {
            var count = await ProjectOrchestrator.ImportProjectsAsync(importedData);
            Snackbar.Add($"Successfully imported {count} projects", Severity.Success);
            await MudDialog.CloseAsync(DialogResult.Ok(count));
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Import failed: {ex.Message}", Severity.Error);
        }
    }
}
```

---

## Implementation Checklist

### Phase 0: Rich Domain Model Setup

- [ ] Create domain entities with business methods
  - [ ] Task aggregate: Complete(), AddSubTask(), MoveToProject(), SetPriority(), ToggleFocus(), UpdateTitle(), UpdateNote(), SetStatus()
  - [ ] Project aggregate: AddTask(), RemoveTask(), UpdateViewType(), GetTaskCount(), UpdateName(), UpdateColor(), UpdateIcon()
- [ ] Create value objects (enums)
  - [ ] TaskPriority (Low, Medium, High)
  - [ ] TaskStatus (ToDo, InProgress, Done)
  - [ ] ProjectViewType (List, Board)
- [ ] Create repository interfaces in domain layer
  - [ ] ITaskRepository
  - [ ] IProjectRepository
- [ ] Create repository implementations in infrastructure layer
- [ ] Create minimal orchestrators in application layer
- [ ] Set up DI container
- [ ] Configure MudBlazor theme

### Phase 1: Core Features

- [ ] Implement UI components using domain entities directly
  - [ ] ProjectSidebar
  - [ ] TaskItem (list view)
  - [ ] TaskCard (board view)
  - [ ] PriorityBadge
  - [ ] FocusButton
  - [ ] EmptyState
- [ ] Implement pages with orchestrator injection
  - [ ] Index.razor (project management)
  - [ ] Tasks.razor (task view)
- [ ] Implement keyboard shortcuts
- [ ] Implement toast notifications
- [ ] Dark theme with Turquoise accent

### Phase 2: Board View + SubTasks

- [ ] Implement BoardView component
- [ ] Implement drag-and-drop
- [ ] Implement view switcher
- [ ] Implement SubTaskList component
- [ ] Update domain methods for subtasks

### Phase 3: My Task Flow + Focus Timer + Import/Export

- [ ] Create FocusSession aggregate
- [ ] Implement FocusTimer component
- [ ] Create smart views (Today, Upcoming, Recent)
- [ ] Create export/import DTOs (ProjectExportDto, TaskExportDto)
- [ ] Implement ExportProjectsAsync in ProjectOrchestrator
- [ ] Implement ImportProjectsAsync in ProjectOrchestrator (ID-based add/update)
- [ ] Create ProjectExport component
- [ ] Create ProjectImport component
- [ ] Test export/import workflow

---

## Common Pitfalls to Avoid

### ❌ Anemic Domain Model

```csharp
// BAD - Anemic domain
public class Task
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
}

// Business logic in service
public class TaskService
{
    public void CompleteTask(Task task)
    {
        task.IsCompleted = true;
        task.CompletedAt = DateTime.UtcNow;
    }
}
```

```csharp
// GOOD - Rich domain
public class Task
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public bool IsCompleted { get; private set; }

    public void Complete()
    {
        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
        Status = TaskStatus.Done;
        foreach (var subTask in SubTasks)
        {
            subTask.Complete();
        }
    }
}

// Orchestrator delegates to domain
public class TaskOrchestrator
{
    public async Task CompleteTaskAsync(Guid taskId)
    {
        var task = await _repository.GetByIdAsync(taskId);
        task.Complete(); // Domain logic
        await _repository.UpdateAsync(task);
    }
}
```

### ❌ DTO Mapping (Don't Do This!)

```csharp
// BAD - DTOs
public class TaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
}

// Mapping layer
public static class TaskMapper
{
    public static TaskDto ToDto(Task task)
    {
        return new TaskDto { Id = task.Id, Title = task.Title };
    }
}
```

```csharp
// GOOD - Use domain aggregates directly
// No mapping needed
@* TaskItem.razor *@
@inject ITaskOrchestrator TaskOrchestrator

[Parameter]
public Task task { get; set; } = null!;

<div>@task.Title</div>
```

### ❌ Business Logic in Orchestrators

```csharp
// BAD - Business logic in orchestrator
public class TaskOrchestrator
{
    public async Task CompleteTaskAsync(Guid taskId)
    {
        var task = await _repository.GetByIdAsync(taskId);
        // Business logic should be in domain!
        task.IsCompleted = true;
        task.CompletedAt = DateTime.UtcNow;
        task.Status = TaskStatus.Done;
        foreach (var subTask in task.SubTasks)
        {
            subTask.IsCompleted = true;
            subTask.CompletedAt = DateTime.UtcNow;
        }
        await _repository.UpdateAsync(task);
    }
}
```

```csharp
// GOOD - Business logic in domain
public class Task
{
    public void Complete()
    {
        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
        Status = TaskStatus.Done;
        foreach (var subTask in SubTasks)
        {
            subTask.Complete();
        }
    }
}

public class TaskOrchestrator
{
    public async Task CompleteTaskAsync(Guid taskId)
    {
        var task = await _repository.GetByIdAsync(taskId);
        task.Complete(); // Delegates to domain
        await _repository.UpdateAsync(task);
    }
}
```

---

## Testing Guidelines

### Testing Stack
- **Framework**: xUnit
- **Pattern**: AAA (Arrange, Act, Assert)
- **Assertion Library**: Shouldly
- **Mocking**: Moq (for orchestrator tests)
- **In-Memory Database**: EF Core In-Memory (for repository tests)

### Unit Testing Domain (xUnit + Shouldly)

```csharp
using Shouldly;

[Fact]
public void Complete_ShouldSetCompletedAndCompleteSubTasks()
{
    // Arrange
    var parentTask = new Task { Id = Guid.NewGuid(), Title = "Parent" };
    var subTask1 = new Task { Id = Guid.NewGuid(), Title = "Sub1" };
    var subTask2 = new Task { Id = Guid.NewGuid(), Title = "Sub2" };
    parentTask.AddSubTask(subTask1);
    parentTask.AddSubTask(subTask2);

    // Act
    parentTask.Complete();

    // Assert
    parentTask.IsCompleted.ShouldBeTrue();
    parentTask.CompletedAt.ShouldNotBeNull();
    parentTask.Status.ShouldBe(TaskStatus.Done);
    subTask1.IsCompleted.ShouldBeTrue();
    subTask2.IsCompleted.ShouldBeTrue();
}
```

### Unit Testing Orchestrators (xUnit + Shouldly + Moq)

```csharp
using Shouldly;
using Moq;

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

### Integration Testing Repositories (xUnit + Shouldly + In-Memory DB)

```csharp
using Shouldly;

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

---

## Performance Considerations

1. **Use AsNoTracking for read-only queries** in repositories
2. **Include navigation properties** when needed (SubTasks, Project)
3. **Index key columns** in DbContext configuration
4. **Use appropriate caching** for frequently accessed data

---

## Deployment Checklist

- [ ] Build Docker image for ARM64
- [ ] Configure Tailscale sidecar container
- [ ] Configure Nginx reverse proxy
- [ ] Set up named volume for database
- [ ] Configure environment variables
- [ ] Test deployment on Raspberry Pi 5
- [ ] Verify Tailscale connectivity
- [ ] Test all features end-to-end

---

## Questions and Clarifications

If you encounter any issues or need clarification:
1. Check this document first
2. Review REQUIREMENTS.md for feature details
3. Review SYSTEMDESIGN.md for architecture details
4. Ask for clarification before making architectural changes

**Remember**: The goal is a rich domain model where business logic lives in entities, orchestrators are minimal, and no DTOs are used.
