using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using Serilog;
using TaskFlow.Presentation.Components;
using TaskFlow.Infrastructure;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Presentation.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
});
builder.Services.AddScoped<IAppExceptionHandler, AppExceptionHandler>();
builder.Services.AddTransient<IUndoManager, UndoManager>();
builder.Services.AddScoped<FocusTimerStateService>();

builder.Services.AddTaskFlowApplicationServices();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = "Data Source=taskflow.db";
}

Log.Information("Starting TaskFlow host with environment {EnvironmentName}", builder.Environment.EnvironmentName);
Log.Information("Using SQLite connection string {ConnectionString}", connectionString);
var appName = builder.Environment.ApplicationName;
var appVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown";
var contentRoot = builder.Environment.ContentRootPath;

builder.Services.AddTaskFlowInfrastructure(connectionString);

var app = builder.Build();

try
{
    await LogMigrationSummaryAsync(app.Services);
    Log.Information("Applying database migrations and seeding if required.");
    await app.Services.InitializeTaskFlowDatabaseAsync();
    Log.Information("Database initialization complete.");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Database initialization failed.");
    throw;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/health", async (IDbContextFactory<AppDbContext> dbFactory, CancellationToken cancellationToken) =>
{
    await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
    var canConnect = await db.Database.CanConnectAsync(cancellationToken);
    return Results.Ok(new
    {
        status = canConnect ? "ok" : "degraded",
        db = canConnect ? "up" : "down"
    });
});

app.Lifetime.ApplicationStarted.Register(() =>
{
    Log.Information("TaskFlow {AppName} v{AppVersion} started. Content root: {ContentRoot}", appName, appVersion, contentRoot);
    var urls = app.Urls.Count == 0 ? "(no urls configured)" : string.Join(", ", app.Urls);
    Log.Information("Startup complete. Listening on {Urls}.", urls);
});

app.Run();

static async Task LogMigrationSummaryAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var db = await dbContextFactory.CreateDbContextAsync();

    var applied = await db.Database.GetAppliedMigrationsAsync();
    var pending = await db.Database.GetPendingMigrationsAsync();

    var appliedList = applied.ToList();
    var pendingList = pending.ToList();

    Log.Information("Migrations summary: applied {AppliedCount}, pending {PendingCount}.", appliedList.Count, pendingList.Count);

    if (pendingList.Count > 0)
    {
        Log.Information("Pending migrations: {PendingMigrations}", string.Join(", ", pendingList));
    }
}
