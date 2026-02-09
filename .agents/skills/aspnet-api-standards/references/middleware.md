# Middleware Pipeline Configuration

## Critical Order

The order of middleware is **critical**. Each middleware processes the request in order and responses in reverse order.

```csharp
var app = builder.Build();

// 1. HTTPS redirection (security first)
app.UseHttpsRedirection();

// 2. Static files (if any) - serve before routing
app.UseStaticFiles();

// 3. Routing - MUST come before CORS, auth, and authorization
app.UseRouting();

// 4. CORS (after routing, before authentication/authorization)
app.UseCors("PolicyName");

// 5. Rate limiting (before authentication to limit anonymous requests)
app.UseRateLimiter();

// 6. Authentication (verify identity)
app.UseAuthentication();

// 7. Authorization (verify permissions)
app.UseAuthorization();

// 8. Response caching
app.UseResponseCaching();

// 9. Response compression
app.UseResponseCompression();

// 10. Status code pages (optional)
app.UseStatusCodePages();

// 11. Endpoints - Controllers handle exceptions via AppExceptionFilterAttribute
app.MapControllers();

// 12. Health checks endpoint
app.MapHealthChecks("/health");

app.Run();
```

## Middleware Explanations

### HTTPS Redirection
```csharp
app.UseHttpsRedirection();
```
Redirects HTTP requests to HTTPS. Place first for security.

### Static Files
```csharp
app.UseStaticFiles();
```
Serves static files (CSS, JS, images) from `wwwroot`. Place before routing to avoid unnecessary route matching.

### Routing
```csharp
app.UseRouting();
```
Matches requests to endpoints. **MUST** come before CORS, authentication, and authorization.

### CORS
```csharp
app.UseCors("PolicyName");
```
Handles cross-origin requests. **MUST** come after `UseRouting()` and before `UseAuthentication()`.

### Rate Limiting
```csharp
app.UseRateLimiter();
```
Limits request rate. Place before authentication to limit anonymous requests.

### Authentication
```csharp
app.UseAuthentication();
```
Verifies user identity (JWT, cookies, etc.). **MUST** come before `UseAuthorization()`.

### Authorization
```csharp
app.UseAuthorization();
```
Verifies user permissions. **MUST** come after `UseAuthentication()`.

### Response Caching
```csharp
app.UseResponseCaching();
```
Caches responses for performance. Place after authorization.

### Response Compression
```csharp
app.UseResponseCompression();
```
Compresses responses (gzip, brotli). Place after caching.

### Status Code Pages
```csharp
app.UseStatusCodePages();
```
Handles status code errors (404, 401, etc.) that don't reach controllers.

### Endpoints
```csharp
app.MapControllers();
```
Maps controller endpoints. Place near the end.

## Exception Handling

**Do NOT use `app.UseExceptionHandler()` middleware.**

Use the global `AppExceptionFilterAttribute` registered in `AddControllers`:

```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AppExceptionFilterAttribute>();
});
```

The exception filter executes within the MVC pipeline and provides:
- Access to MVC-specific context (ActionContext, ModelState)
- Type-safe exception handling with structured logging
- Consistent error responses across all controllers
- Better integration with FluentValidation and domain exceptions

## Minimal Pipeline (Simple API)

For simple APIs without security or advanced features:

```csharp
var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

app.Run();
```

## Development-Specific Middleware

```csharp
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Detailed error pages
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// ... rest of pipeline
```

## Custom Middleware

### Inline Middleware

```csharp
app.Use(async (context, next) =>
{
    // Before request processing
    Console.WriteLine($"Request: {context.Request.Path}");
    
    await next(); // Call next middleware
    
    // After response processing
    Console.WriteLine($"Response: {context.Response.StatusCode}");
});
```

### Custom Middleware Class

```csharp
public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();
        _logger.LogInformation(
            "Request {Method} {Path} completed in {ElapsedMilliseconds}ms",
            context.Request.Method,
            context.Request.Path,
            stopwatch.ElapsedMilliseconds);
    }
}

// Register
app.UseMiddleware<RequestTimingMiddleware>();
```

### Extension Method for Middleware

```csharp
public static class RequestTimingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestTiming(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestTimingMiddleware>();
    }
}

// Usage
app.UseRequestTiming();
```

## Rate Limiting Configuration

```csharp
// Configure rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var app = builder.Build();
app.UseRateLimiter();
```

## Response Caching Configuration

```csharp
// Configure response caching
builder.Services.AddResponseCaching();

var app = builder.Build();
app.UseResponseCaching();
```

```csharp
// Use in controllers
[HttpGet]
[ResponseCache(Duration = 60)] // Cache for 60 seconds
public async Task<IActionResult> GetProducts()
{
    var products = await mediator.Send(new GetProductsQuery());
    return Ok(products);
}
```

## Response Compression Configuration

```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

var app = builder.Build();
app.UseResponseCompression();
```

## Health Checks

```csharp
// Configure health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddUrlGroup(new Uri("https://api.example.com"), "External API");

var app = builder.Build();
app.MapHealthChecks("/health");

// Detailed health check
app.MapHealthChecks("/health/detailed", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

## Best Practices

- **ALWAYS** follow the documented middleware order
- **NEVER** place `UseAuthentication()` before `UseRouting()`
- **NEVER** place `UseAuthorization()` before `UseAuthentication()`
- **NEVER** place `UseCors()` before `UseRouting()`
- Use `UseExceptionHandler()` only for non-MVC scenarios (use `AppExceptionFilterAttribute` for controllers)
- Keep custom middleware lightweight and focused
- Use dependency injection in custom middleware constructors
- Test middleware order changes in development before deploying
