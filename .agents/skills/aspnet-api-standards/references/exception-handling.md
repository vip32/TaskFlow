# Exception Handling

## Global Exception Filter Pattern

Use `AppExceptionFilterAttribute` for centralized exception handling across all controllers.

### Implementation

```csharp
[AttributeUsage(AttributeTargets.All)]
public sealed class AppExceptionFilterAttribute : ExceptionFilterAttribute
{
    private readonly ILogger<Exception> _logger;
    private readonly IConfiguration _configuration;

    public AppExceptionFilterAttribute(ILogger<Exception> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public override void OnException(ExceptionContext context)
    {
        var type = "{Type}";

        // Distinguish between domain and system exceptions
        if (context.Exception is not DomainException)
        {
            // System/Infrastructure exceptions
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            _logger.LogError(
                context.Exception,
                $"{context.Exception.Message}-{type}",
                _configuration.GetSection("NameExceptionSystem").GetValue<string>("Type"));
        }
        else
        {
            // Domain/Business exceptions
            _logger.LogError(
                context.Exception,
                $"{context.Exception.Message}-{type}",
                _configuration.GetSection("NameExceptionDomain").GetValue<string>("Type"));
        }

        // Log full stack trace
        _logger.LogError(context.Exception, context.Exception.Message, new[]
        {
            context.Exception.StackTrace
        });

        // Return structured error response
        var errorResponse = new
        {
            context.Exception.Message,
            ExceptionType = context.Exception.GetType().ToString(),
            TraceId = context.HttpContext.TraceIdentifier
        };

        context.Result = new ObjectResult(errorResponse)
        {
            StatusCode = context.HttpContext.Response.StatusCode
        };
    }
}
```

### Registration

```csharp
// Program.cs
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AppExceptionFilterAttribute>();
});
```

### Configuration

```json
// appsettings.json
{
  "NameExceptionSystem": {
    "Type": "SystemException"
  },
  "NameExceptionDomain": {
    "Type": "DomainException"
  }
}
```

## Custom Domain Exceptions

Create exception types in the **Domain** layer.

### Base Exception

```csharp
public abstract class DomainException : Exception
{
    public int? StatusCode { get; protected set; }

    protected DomainException(string message, int? statusCode = null)
        : base(message)
    {
        StatusCode = statusCode;
    }

    protected DomainException(string message, Exception innerException, int? statusCode = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
```

### Specific Exceptions

```csharp
// 404 Not Found
public class NotFoundException : DomainException
{
    public NotFoundException(string message)
        : base(message, StatusCodes.Status404NotFound)
    {
    }

    public NotFoundException(string entityName, object key)
        : base($"{entityName} with id '{key}' was not found.", StatusCodes.Status404NotFound)
    {
    }
}

// 400 Bad Request - Validation
public class ValidationException : DomainException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation failures occurred.", StatusCodes.Status400BadRequest)
    {
        Errors = errors;
    }

    public ValidationException(string field, string error)
        : base($"Validation failed for {field}.", StatusCodes.Status400BadRequest)
    {
        Errors = new Dictionary<string, string[]>
        {
            { field, new[] { error } }
        };
    }
}

// 400 Bad Request - Business Rule
public class BusinessRuleViolationException : DomainException
{
    public string RuleName { get; }

    public BusinessRuleViolationException(string ruleName, string message)
        : base(message, StatusCodes.Status400BadRequest)
    {
        RuleName = ruleName;
    }
}

// 401 Unauthorized
public class UnauthorizedDomainException : DomainException
{
    public UnauthorizedDomainException(string message)
        : base(message, StatusCodes.Status401Unauthorized)
    {
    }
}

// 403 Forbidden
public class ForbiddenException : DomainException
{
    public ForbiddenException(string message)
        : base(message, StatusCodes.Status403Forbidden)
    {
    }
}

// 409 Conflict
public class ConflictException : DomainException
{
    public ConflictException(string message)
        : base(message, StatusCodes.Status409Conflict)
    {
    }
}
```

### Usage in Handlers

```csharp
public class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        // Throw NotFoundException if customer doesn't exist
        var customer = await customerRepository.GetByIdAsync(command.CustomerId, ct)
            ?? throw new NotFoundException(nameof(Customer), command.CustomerId);

        // Throw BusinessRuleViolationException for business rules
        if (command.Items.Count == 0)
            throw new BusinessRuleViolationException(
                "OrderMustHaveItems",
                "An order must contain at least one item.");

        // Throw ConflictException for conflicts
        var existingOrder = await orderRepository.GetActiveOrder(customer.Id, ct);
        if (existingOrder != null)
            throw new ConflictException($"Customer {customer.Id} already has an active order.");

        var order = Order.Create(customer, command.Items);

        await orderRepository.AddAsync(order, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return order.Id;
    }
}
```

## Important Notes

- **NEVER** use try-catch in controllers - let the global filter handle exceptions
- **NEVER** use `app.UseExceptionHandler()` middleware - use `AppExceptionFilterAttribute` instead
- The exception filter runs within the MVC pipeline, providing access to MVC-specific context
- All domain exceptions should be created in the Domain layer
- System/infrastructure exceptions are caught and logged as 500 Internal Server Error
