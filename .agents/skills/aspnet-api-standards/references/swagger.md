# Swagger/OpenAPI Configuration

## Basic Setup

```csharp
// Program.cs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

## Complete Configuration with Metadata

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1",
        Description = "A comprehensive API for managing resources",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@example.com",
            Url = new Uri("https://support.example.com")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        },
        TermsOfService = new Uri("https://example.com/terms")
    });
});
```

## JWT Bearer Authentication in Swagger

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1"
    });

    // Add JWT Bearer authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
```

## XML Documentation Comments

```csharp
// Enable XML documentation
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});
```

```xml
<!-- Enable XML documentation in .csproj -->
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn> <!-- Suppress missing XML comment warnings -->
</PropertyGroup>
```

```csharp
/// <summary>
/// Creates a new order
/// </summary>
/// <param name="command">The order creation details</param>
/// <param name="cancellationToken">Cancellation token</param>
/// <returns>The created order ID</returns>
/// <response code="201">Order created successfully</response>
/// <response code="400">Invalid request data</response>
/// <response code="404">Customer not found</response>
[HttpPost]
[ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> CreateOrder(
    [FromBody] CreateOrderCommand command,
    CancellationToken cancellationToken)
{
    var orderId = await mediator.Send(command, cancellationToken);
    return CreatedAtAction(nameof(GetOrder), new { id = orderId }, orderId);
}
```

## Custom Swagger UI Settings

```csharp
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root (https://localhost:5001/)
        options.DocumentTitle = "My API Documentation";
        options.DefaultModelsExpandDepth(-1); // Hide schemas section
        options.DocExpansion(DocExpansion.None); // Collapse all operations by default
        options.DisplayRequestDuration(); // Show request duration
    });
}
```

## Multiple API Versions

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1",
        Description = "Version 1 of the API"
    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "My API",
        Version = "v2",
        Description = "Version 2 of the API"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "My API V2");
    });
}
```

```csharp
// Controllers specify their version
[ApiController]
[Route("api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "v1")]
public class OrdersV1Controller : ControllerBase { }

[ApiController]
[Route("api/v2/[controller]")]
[ApiExplorerSettings(GroupName = "v2")]
public class OrdersV2Controller : ControllerBase { }
```

## Custom Operation Filters

```csharp
// Add custom headers to all operations
public class AddHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Correlation-ID",
            In = ParameterLocation.Header,
            Required = false,
            Schema = new OpenApiSchema { Type = "string" }
        });
    }
}

// Register the filter
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    options.OperationFilter<AddHeaderOperationFilter>();
});
```

## Swagger in Production (Optional)

```csharp
var app = builder.Build();

// Enable Swagger in production with authentication
if (app.Configuration.GetValue<bool>("Swagger:EnableInProduction"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}
```

```json
// appsettings.Production.json
{
  "Swagger": {
    "EnableInProduction": false
  }
}
```

## Response Examples

```csharp
[HttpGet("{id:guid}")]
[ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[SwaggerResponse(200, "Returns the order", typeof(OrderDto))]
[SwaggerResponse(404, "Order not found")]
public async Task<IActionResult> GetOrder(Guid id)
{
    var order = await mediator.Send(new GetOrderByIdQuery(id));
    return order == null ? NotFound() : Ok(order);
}
```

## Best Practices

- **Development only**: Only enable Swagger in development by default
- **Use XML comments**: Document controllers and DTOs for better Swagger documentation
- **ProducesResponseType**: Always declare response types for accurate documentation
- **Security schemes**: Configure JWT Bearer authentication for protected endpoints
- **Versioning**: Use multiple Swagger documents for multiple API versions
- **Hide sensitive endpoints**: Use `[ApiExplorerSettings(IgnoreApi = true)]` to exclude endpoints
