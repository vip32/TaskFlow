# Security Configuration

## CORS (Cross-Origin Resource Sharing)

### Basic Configuration

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("https://app.example.com", "https://admin.example.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();
app.UseCors("AllowSpecificOrigins"); // MUST be after UseRouting()
```

### Development vs Production

```csharp
builder.Services.AddCors(options =>
{
    // Production policy - specific origins
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("https://app.example.com", "https://admin.example.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
    
    // Development policy - allow all
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCors(app.Environment.IsDevelopment() ? "AllowAll" : "AllowSpecificOrigins");
```

### Specific Methods and Headers

```csharp
options.AddPolicy("RestrictedPolicy", policy =>
{
    policy.WithOrigins("https://app.example.com")
          .WithMethods("GET", "POST")
          .WithHeaders("Content-Type", "Authorization")
          .AllowCredentials();
});
```

### Per-Controller CORS

```csharp
// Apply to specific controller
[EnableCors("AllowSpecificOrigins")]
public class ProductsController : ControllerBase
{
    // ...
}

// Disable CORS on specific action
[DisableCors]
[HttpGet("internal")]
public IActionResult InternalEndpoint()
{
    // ...
}
```

## Authentication & Authorization

### JWT Bearer Authentication

```csharp
// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
    });

var app = builder.Build();
app.UseAuthentication(); // MUST be before UseAuthorization()
app.UseAuthorization();
```

### Role-Based Authorization

```csharp
// Program.cs - Define policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Manager", "Admin"));
    options.AddPolicy("RequireEmail", policy => policy.RequireClaim("email"));
});
```

### Protecting Endpoints

```csharp
// Require authentication
[Authorize]
[HttpGet]
public async Task<IActionResult> GetOrders()
{
    // Only authenticated users can access
}

// Require specific role
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteOrder(Guid id)
{
    // Only Admin role can access
}

// Require policy
[Authorize(Policy = "AdminOnly")]
[HttpPost("sensitive")]
public async Task<IActionResult> SensitiveOperation()
{
    // Only users matching AdminOnly policy can access
}

// Allow anonymous (overrides controller-level [Authorize])
[AllowAnonymous]
[HttpGet("public")]
public IActionResult PublicEndpoint()
{
    // Anyone can access
}
```

### Controller-Level Authorization

```csharp
// All actions in controller require authentication
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    // This requires authentication
    [HttpGet]
    public async Task<IActionResult> GetOrders() { }

    // This is public despite controller-level [Authorize]
    [AllowAnonymous]
    [HttpGet("public")]
    public IActionResult GetPublicOrders() { }
}
```

### Custom Authorization Policies

```csharp
// Custom requirement
public class MinimumAgeRequirement : IAuthorizationRequirement
{
    public int MinimumAge { get; }
    public MinimumAgeRequirement(int minimumAge) => MinimumAge = minimumAge;
}

// Custom handler
public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MinimumAgeRequirement requirement)
    {
        var birthDateClaim = context.User.FindFirst(c => c.Type == "birthdate");
        if (birthDateClaim == null)
            return Task.CompletedTask;

        var birthDate = DateTime.Parse(birthDateClaim.Value);
        var age = DateTime.Today.Year - birthDate.Year;

        if (age >= requirement.MinimumAge)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

// Register in Program.cs
builder.Services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AtLeast18", policy =>
        policy.Requirements.Add(new MinimumAgeRequirement(18)));
});

// Usage
[Authorize(Policy = "AtLeast18")]
[HttpGet("adult-content")]
public IActionResult AdultContent() { }
```

## HTTPS Configuration

### Force HTTPS

```csharp
// Program.cs
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts(); // Add HSTS header
}

app.UseHttpsRedirection(); // Redirect HTTP to HTTPS
```

### Configure HTTPS Port

```json
// appsettings.json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:5001"
      },
      "Http": {
        "Url": "http://localhost:5000"
      }
    }
  }
}
```

## API Keys (Alternative Authentication)

```csharp
// Custom middleware for API key authentication
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-API-Key";

    public ApiKeyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("API Key missing");
            return;
        }

        var apiKey = configuration.GetValue<string>("ApiKey");
        if (!apiKey.Equals(extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid API Key");
            return;
        }

        await _next(context);
    }
}

// Register in Program.cs
app.UseMiddleware<ApiKeyMiddleware>();
```

## Best Practices

- **ALWAYS** use HTTPS in production
- **ALWAYS** place `UseAuthentication()` before `UseAuthorization()`
- **ALWAYS** place `UseCors()` after `UseRouting()` and before `UseAuthentication()`
- Store secrets in environment variables or Azure Key Vault, NEVER in code
- Use strong JWT secret keys (at least 256 bits)
- Set appropriate token expiration times
- Validate all JWT parameters (issuer, audience, lifetime, signing key)
- Use `[Authorize]` at controller level, `[AllowAnonymous]` for specific exceptions
