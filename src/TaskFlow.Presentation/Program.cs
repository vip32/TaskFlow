using Serilog;
using TaskFlow.Application;
using TaskFlow.Presentation.Components;
using TaskFlow.Infrastructure;

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

builder.Services.AddTaskFlowApplicationServices();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = "Data Source=taskflow.db";
}

Log.Information("Starting TaskFlow host with environment {EnvironmentName}", builder.Environment.EnvironmentName);
Log.Information("Using SQLite connection string {ConnectionString}", connectionString);

builder.Services.AddTaskFlowInfrastructure(connectionString);

var app = builder.Build();

try
{
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
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var urls = app.Urls.Count == 0
        ? "(no urls configured)"
        : string.Join(", ", app.Urls);
    Log.Information("Startup complete. Listening on {Urls}.", urls);
});

app.Run();
