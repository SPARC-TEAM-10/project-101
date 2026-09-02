# DOTNET-RULES — .NET / ASP.NET Core Coding Standards

All rules in this document are **binding** for every agent. Read this file in full before planning, implementing, or reviewing any .NET code.

---

# Part 1 — API Development Standards (ASP.NET Core)

## 1. Project Structure & Layering

Use clean architecture with four projects:

| Project | Responsibility |
|---|---|
| `[Project].API` | Controllers, Middleware, Filters, `Program.cs`, DI registration |
| `[Project].Application` | Services, Interfaces, DTOs, Validators, Mappings |
| `[Project].Domain` | Entities, Value Objects, Domain Exceptions, Enums, Constants |
| `[Project].Infrastructure` | Repositories, `DbContext`, EF Configurations, External HTTP Clients |

**Layer isolation rule:** `Controller` → `IService` → `IRepository`. No layer may skip the one below it. Controllers never reference `DbContext` or repository types directly.

---

## 2. Controller Standards

- Inherit from `ControllerBase` (not `Controller` — no Razor views in API projects)
- Decorate with `[ApiController]` and `[Route("api/v{version:apiVersion}/[controller]")]`
- All actions must return `async Task<ActionResult<T>>` or `async Task<IActionResult>`
- Annotate every action with `[ProducesResponseType]` for every possible status code
- Delegate all logic to the service layer — zero business logic in controllers
- Never access `DbContext` directly from a controller
- Always accept and forward `CancellationToken`

```csharp
[ApiController]
[Authorize]
[Route("api/v{version:apiVersion}/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderResponse>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _orderService.CreateOrderAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetOrderById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetOrderByIdAsync(id, cancellationToken);
        return Ok(result);
    }
}
```

---

## 3. API Versioning

- Use URL path versioning: `/api/v1/`, `/api/v2/`
- Register with `Microsoft.AspNetCore.Mvc.Versioning`
- Declare supported versions on the controller: `[ApiVersion("1.0")]`
- Mark deprecated versions: `[ApiVersion("1.0", Deprecated = true)]`
- Never remove an old version without a deprecation window
- Default API version must always be set in `Program.cs`

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});
```

---

## 4. Request / Response DTOs

- All request DTOs are `record` types using `required` properties (C# 12+)
- All response DTOs are `record` types or immutable classes
- Use `[JsonPropertyName]` for explicit JSON serialisation control
- Never expose domain entities directly in API responses — always map to a DTO
- Separate request types from response types — never reuse one DTO for both

```csharp
// Request DTO
public record CreateOrderRequest
{
    public required Guid CustomerId { get; init; }
    public required List<CreateOrderItemRequest> Items { get; init; }
    public required Guid ShippingAddressId { get; init; }
}

// Response DTO
public record OrderResponse
{
    public required Guid Id { get; init; }
    public required string Status { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required IReadOnlyList<OrderItemResponse> Items { get; init; }
}
```

---

## 5. Validation

- Use FluentValidation for all request validation
- Register validators with `AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())`
- Integrate with the built-in model-state pipeline via `AddFluentValidationAutoValidation()`
- Return `ValidationProblemDetails` (RFC 7807) on validation failure — this is automatic with `[ApiController]`
- Never validate inside the service layer — validation is a controller-boundary concern

```csharp
public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("CustomerId is required.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("Order must contain at least one item.");
        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemRequestValidator());
        RuleFor(x => x.ShippingAddressId).NotEmpty().WithMessage("ShippingAddressId is required.");
    }
}
```

---

## 6. Error Handling

- Implement a global exception handler using `IExceptionHandler` (.NET 8) or `UseExceptionHandler` middleware
- Map domain exceptions to HTTP status codes exclusively in the exception handler — never in services or controllers
- Never expose raw exception messages, stack traces, or internal identifiers in production responses
- Return `ProblemDetails` (RFC 7807) for all error responses

### Domain-Exception-to-HTTP Mapping

| Domain Exception | HTTP Status |
|---|---|
| `NotFoundException` | 404 Not Found |
| `ValidationException` | 422 Unprocessable Entity |
| `ConflictException` | 409 Conflict |
| `ForbiddenException` | 403 Forbidden |
| `UnauthorizedException` | 401 Unauthorized |
| Unhandled `Exception` | 500 Internal Server Error (generic message only) |

```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        var (status, title) = ex switch
        {
            NotFoundException e    => (StatusCodes.Status404NotFound, e.Message),
            ConflictException e    => (StatusCodes.Status409Conflict, e.Message),
            ForbiddenException     => (StatusCodes.Status403Forbidden, "Access denied."),
            UnauthorizedException  => (StatusCodes.Status401Unauthorized, "Authentication required."),
            _                      => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        if (status >= 500)
            _logger.LogError(ex, "Unhandled exception for request {Path}", ctx.Request.Path);

        ctx.Response.StatusCode = status;
        await ctx.Response.WriteAsJsonAsync(
            new ProblemDetails { Status = status, Title = title }, ct);
        return true;
    }
}
```

---

## 7. Authentication & Authorization

- Use JWT Bearer authentication via `Microsoft.AspNetCore.Authentication.JwtBearer`
- Store JWT secret, issuer, and audience in configuration — never hardcode
- Apply `[Authorize]` at the controller level by default
- Explicitly mark public endpoints with `[AllowAnonymous]`
- Use policy-based authorization for role/permission checks
- Read user identity from `HttpContext.User.Claims` — never trust request-body claims
- Validate token expiry, issuer, and audience on every request

```csharp
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });
```

---

## 8. HTTP Status Codes

| Scenario | Status Code |
|---|---|
| Resource created | 201 Created |
| Resource retrieved | 200 OK |
| Empty list result | 200 OK (return empty array — never 404) |
| Resource updated | 200 OK (return updated resource) |
| Resource deleted | 204 No Content |
| Invalid request body | 400 Bad Request |
| Unauthenticated | 401 Unauthorized |
| Insufficient permissions | 403 Forbidden |
| Resource not found | 404 Not Found |
| State conflict | 409 Conflict |
| Validation failure | 422 Unprocessable Entity |
| Server error | 500 Internal Server Error |

---

## 9. Async / Await Rules

- All controller actions are `async Task<ActionResult<T>>`
- All service methods are `async Task<T>` or `async Task`
- All repository methods are `async Task<T>` or `async Task`
- Always thread `CancellationToken` from the controller action through the entire call chain
- Never use `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` — causes deadlocks in ASP.NET Core
- Use `ConfigureAwait(false)` in Application and Infrastructure layer code (not in API project)
- Never use `async void` except for event handlers

---

## 10. Dependency Injection

- Register all services and repositories in extension methods (`IServiceCollection.AddApplicationServices()`, `IServiceCollection.AddInfrastructureServices()`)
- Use constructor injection exclusively — no property injection, no `IServiceLocator`, no `HttpContext.RequestServices`
- Lifetime rules:
  - `DbContext` → `Scoped`
  - Repositories → `Scoped`
  - Services → `Scoped`
  - `IHttpClientFactory` typed clients → registered as `Transient` via factory
  - Singletons → only for stateless utilities (e.g., `ILogger<T>`, `IOptions<T>`)
- Never inject scoped services into singleton services (captive dependency)

---

## 11. Structured Logging

- Inject `ILogger<T>` via constructor — never use static loggers or `LogManager`
- Use message templates, not string interpolation:
  ```csharp
  // Correct
  _logger.LogInformation("Order {OrderId} created for customer {CustomerId}", order.Id, order.CustomerId);
  // Wrong — defeats structured logging
  _logger.LogInformation($"Order {order.Id} created");
  ```
- Log levels:
  - `LogDebug` — diagnostic / trace info
  - `LogInformation` — normal business events
  - `LogWarning` — handled errors, degraded behaviour
  - `LogError` — unhandled exceptions, data loss risk
- Never log: passwords, tokens, secrets, credit card numbers, PII

---

## 12. Pagination

All list endpoints must return a `PagedResponse<T>`:

```csharp
public record PagedResponse<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
```

Query parameters: `?page=1&pageSize=20` (default `pageSize` = 20, max = 100).

---

## 13. Date/Time Conventions

| Scenario | C# Type | SQL Column Type | EF Core Mapping |
|---|---|---|---|
| Full timestamp with timezone | `DateTimeOffset` | `datetimeoffset` | Default |
| UTC timestamp (no timezone) | `DateTime` (UTC only) | `datetime2` | Default |
| Date only (no time) | `DateOnly` | `date` | `.HasColumnType("date")` |
| Time only (no date) | `TimeOnly` | `time` | `.HasColumnType("time")` |

- Always store and transmit dates in UTC — use `DateTime.UtcNow`, never `DateTime.Now`
- Serialize as ISO 8601
- **Never use `datetime`** — always `datetime2` for better precision and range
- Use `DateOnly` for date-of-birth, contract dates, calendar dates — never `DateTime` with time truncated
- Use `TimeOnly` for opening/closing times, scheduled daily tasks

---

## 14. JSON Serialisation

Configure globally in `Program.cs`:

```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
```

Rules:
- All JSON property names are `camelCase`
- `null` properties are omitted from responses
- Enums are serialised as strings, not integers
- Never use `dynamic` or `JObject` / `JToken` in response mappings

---

## 15. CORS

- Never configure `AllowAnyOrigin()` for non-public APIs
- Define a named policy and apply it explicitly
- List allowed origins in `appsettings.json`

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()!)
              .AllowAnyMethod()
              .AllowAnyHeader());
});
```

---

# Part 2 — SQL Database Design Guidelines (SQL Server + EF Core)

## 1. Naming Conventions

### Tables
- Use **PascalCase** singular nouns: `Order`, `OrderItem`, `Customer`
- Junction/link tables: combine both entity names: `OrderProduct`, `UserRole`
- Never use underscores or abbreviations in table names
- Never prefix with `tbl_` or similar

### Columns
- Use **PascalCase**: `OrderId`, `CreatedAt`, `IsDeleted`
- Primary key: always `Id` (not `OrderId` on the `Order` table — just `Id`)
- Foreign keys: `{ReferencedEntity}Id` → e.g., `CustomerId`, `ShippingAddressId`
- Boolean columns: prefix with `Is` or `Has`: `IsActive`, `IsDeleted`, `HasDiscount`
- Date columns: suffix with `At` or `On`: `CreatedAt`, `UpdatedAt`, `DeletedAt`, `ShippedOn`

### Indexes
- Primary key index: auto-generated by EF Core, no manual name needed
- Foreign key indexes: `IX_{Table}_{Column}` → e.g., `IX_Orders_CustomerId`
- Composite indexes: `IX_{Table}_{Col1}_{Col2}`
- Unique indexes: `UX_{Table}_{Column}` → e.g., `UX_Users_Email`

### Stored Procedures (if used)
- `usp_{Verb}{Entity}` → e.g., `usp_GetOrdersByCustomer`, `usp_ArchiveExpiredSessions`

---

## 2. Primary Key Design

- All primary keys are `UNIQUEIDENTIFIER` (GUID) in SQL Server, mapped to `Guid` in C#
- Use `NEWSEQUENTIALID()` as the default on the server side to maintain insert performance with clustered indexes
- Never use integer identity PKs on entities exposed via API — prevents enumeration attacks and simplifies distributed ID generation
- EF Core mapping:

```csharp
builder.Property(e => e.Id)
    .HasDefaultValueSql("NEWSEQUENTIALID()");
```

---

## 3. Base Entity Pattern

All domain entities inherit from `BaseEntity`:

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
}
```

EF Core base configuration:

```csharp
public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T>
    where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()").ValueGeneratedOnAdd();
        builder.Property(e => e.CreatedBy).HasMaxLength(256).IsRequired();
        builder.Property(e => e.UpdatedAt).ValueGeneratedOnUpdate();
        builder.Property(e => e.UpdatedBy).HasMaxLength(256);
    }
}
```

---

## 4. Soft Delete

- Implement soft delete with `IsDeleted` flag — never hard-delete business data
- Add a global query filter in EF Core to exclude soft-deleted rows automatically:

```csharp
// In entity configuration:
builder.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired();

// In DbContext:
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(
                e => !EF.Property<bool>(e, "IsDeleted"));
    }
}
```

- Add a filtered index to exclude soft-deleted rows from scans:

```csharp
builder.HasIndex(e => e.IsDeleted).HasFilter("[IsDeleted] = 0");
```

---

## 5. EF Core Configuration Rules

- Use Fluent API in `IEntityTypeConfiguration<T>` exclusively — never Data Annotations on domain entities
- One configuration class per entity in `Infrastructure/Data/Configurations/`
- Register all configurations with `modelBuilder.ApplyConfigurationsFromAssembly()`
- Never put navigation property configuration in the entity class itself

### Entity Configuration Pattern:

```csharp
public class OrderConfiguration : BaseEntityConfiguration<Order>
{
    public override void Configure(EntityTypeBuilder<Order> builder)
    {
        base.Configure(builder);

        builder.ToTable("Orders");

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.TotalAmount)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.HasOne(e => e.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.CustomerId)
            .HasDatabaseName("IX_Orders_CustomerId");
    }
}
```

---

## 6. Column Type Mapping

| C# Type | SQL Server Type | EF Core Configuration |
|---|---|---|
| `Guid` | `uniqueidentifier` | Default |
| `string` (required) | `nvarchar(n)` | `.HasMaxLength(n).IsRequired()` |
| `string?` | `nvarchar(n) NULL` | `.HasMaxLength(n)` |
| `decimal` | `decimal(18,6)` | `.HasPrecision(18, 6)` |
| `DateTime` (UTC) | `datetime2` | Default |
| `DateTimeOffset` | `datetimeoffset` | Default |
| `bool` | `bit` | `.HasDefaultValue(false)` |
| `enum` | `nvarchar(50)` | `.HasConversion<string>().HasMaxLength(50)` |
| `byte[]` (binary) | `varbinary(max)` | Default |

- **Never use `money` type** in SQL Server — rounding bugs; use `decimal(18,6)` instead
- **Never use `datetime`** — use `datetime2` for better precision and range
- **Never use `nvarchar(MAX)`** unless the column genuinely holds large text (images, JSON blobs)

---

## 7. Indexing Strategy

**Always index:**
- All foreign key columns
- Columns used in `WHERE` clauses on large tables
- Columns used in `ORDER BY` on paginated queries
- Columns used in `JOIN` conditions (beyond PK/FK)

**Consider composite indexes** when queries consistently filter on multiple columns together.

**Avoid over-indexing:**
- Each index has a write cost — do not index every column
- Review execution plans before adding indexes to large tables

**Covering indexes** — use `INCLUDE` for columns frequently returned but not filtered on:

```sql
CREATE INDEX IX_Orders_CustomerId_Status
ON Orders (CustomerId, Status)
INCLUDE (CreatedAt, TotalAmount);
```

EF Core fluent equivalent:

```csharp
builder.HasIndex(e => new { e.CustomerId, e.Status })
    .IncludeProperties(e => new { e.CreatedAt, e.TotalAmount })
    .HasDatabaseName("IX_Orders_CustomerId_Status");
```

---

## 8. Migration Practices

- **Run `dotnet build` and confirm zero errors before running any `dotnet ef migrations` command.** EF Core migration tooling reads the compiled assembly — a build with errors will produce an incorrect or incomplete migration. Do not proceed to migration generation until the build is clean.
- Use EF Core code-first migrations exclusively (no hand-written SQL DDL scripts in source)
- Generate with: `dotnet ef migrations add {MigrationName} --project Infrastructure --startup-project API`
- **Always** manually review the generated migration before committing — autogenerate is a starting point, not final
- **Always** implement `Down()` — every migration must be reversible
- Migration naming: `{YYYYMMDDHHMMSS}_{PascalCaseDescription}` (auto-generated timestamp is fine)
- Never modify an existing migration that has been applied to any environment — always add a new one
- Store migrations in `Infrastructure/Data/Migrations/`
- Run migrations at startup only in development; use `dotnet ef database update` in CI/CD for staging/production

---

## 9. Query Patterns

### Eager Loading (required)

Always use explicit eager loading for navigation properties — never rely on lazy loading:

```csharp
var order = await _context.Orders
    .Include(o => o.Items)
        .ThenInclude(i => i.Product)
    .Include(o => o.Customer)
    .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
```

**Never enable lazy loading** (`UseLazyLoadingProxies`) in the application — it hides N+1 problems.

### Projection with `Select`

When you don't need the full entity, project to a DTO directly in the query to reduce data transfer:

```csharp
var summaries = await _context.Orders
    .Where(o => o.CustomerId == customerId)
    .OrderByDescending(o => o.CreatedAt)
    .Select(o => new OrderSummaryDto
    {
        Id = o.Id,
        Status = o.Status,
        TotalAmount = o.TotalAmount,
        CreatedAt = o.CreatedAt
    })
    .ToListAsync(cancellationToken);
```

### Pagination

Always paginate queries on tables that can grow unbounded:

```csharp
var items = await query
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(cancellationToken);

var total = await query.CountAsync(cancellationToken);
```

### No Raw SQL Except for Performance-Critical Paths

- Prefer LINQ-to-Entities for all queries
- When raw SQL is required (complex reporting, full-text search, bulk operations), use parameterised queries only:

```csharp
// Correct — parameterised
await _context.Database.ExecuteSqlRawAsync(
    "UPDATE Orders SET Status = {0} WHERE Id = {1}", status, id);

// Wrong — SQL injection risk
await _context.Database.ExecuteSqlRawAsync(
    $"UPDATE Orders SET Status = '{status}' WHERE Id = '{id}'");
```

---

## 10. Repository Pattern

All data access goes through typed repository interfaces:

```csharp
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedAsync(
        Guid customerId, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
```

Implementation rules:
- `GetById` returns `T?` (nullable) — never throws `NotFoundException` from the repository
- `Add` / `Update` / `Delete` do not call `SaveChangesAsync` — the Unit of Work (service layer) owns the transaction
- Pass `CancellationToken` to every async EF Core method
- Never expose `IQueryable<T>` through a repository interface — it leaks the ORM into the Application layer

---

## 11. Unit of Work

- Use the `DbContext` as the implicit Unit of Work
- `SaveChangesAsync` is called exactly once per service method, at the end of the operation
- Never call `SaveChangesAsync` inside a repository method
- For operations touching multiple repositories, wrap in a `TransactionScope` or use explicit `DbContext.Database.BeginTransactionAsync()`

---

## 12. DbContext Design

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        SetAuditFields();
        return base.SaveChangesAsync(ct);
    }

    private void SetAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);
        var now = DateTime.UtcNow;
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
                entry.Entity.SetCreatedAt(now);
            entry.Entity.SetUpdatedAt(now);
        }
    }
}
```

---

## 13. Connection String & Performance Configuration

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(30);
        })
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution));
```

- Use `NoTrackingWithIdentityResolution` as the default tracking behaviour for read-heavy workloads
- Explicitly use `AsTracking()` only when you need to modify and save entities

---

## 14. Seeding

- Use `IHostedService` or `app.MigrateAndSeedAsync()` extension for seeding
- Never use `modelBuilder.HasData()` for large seed datasets — it bloats migrations
- Idempotent seed: always check if the record exists before inserting

---

# Part 3 — Unit Testing Guidelines (.NET / C#)

## 1. Test Project Structure

| Project | Contents | Framework |
|---|---|---|
| `[Project].Tests.Unit` | Unit tests for all layers (mocked dependencies) | xUnit + Moq + FluentAssertions |
| `[Project].Tests.Integration` | Integration tests against real HTTP + database | xUnit + WebApplicationFactory + TestContainers |

**Directory layout inside each test project:**

```
[Project].Tests.Unit/
  Controllers/
    OrdersControllerTests.cs
  Services/
    OrderServiceTests.cs
  Repositories/
    OrderRepositoryTests.cs
  Validators/
    CreateOrderRequestValidatorTests.cs
  Fixtures/
    TestFixtures.cs          ← shared builder methods, fake factories
```

---

## 2. Naming Conventions

### Test method naming: `MethodName_StateUnderTest_ExpectedBehavior`

```csharp
// Service tests
CreateOrderAsync_WithValidRequest_ReturnsCreatedOrder
CreateOrderAsync_WhenCustomerNotFound_ThrowsNotFoundException
GetOrderByIdAsync_WhenOrderExists_ReturnsOrder
GetOrderByIdAsync_WhenOrderNotFound_ThrowsNotFoundException

// Controller tests
CreateOrder_WithValidBody_Returns201Created
CreateOrder_WithInvalidBody_Returns422UnprocessableEntity
CreateOrder_WhenUnauthenticated_Returns401Unauthorized

// Validator tests
Validate_WithEmptyItems_ReturnsValidationError
Validate_WithValidRequest_PassesValidation
```

### Test class naming: `{ClassName}Tests`

---

## 3. Arrange-Act-Assert (AAA) Structure

Every test follows the AAA pattern with explicit `// Arrange`, `// Act`, `// Assert` comments:

```csharp
[Fact]
public async Task CreateOrderAsync_WithValidRequest_ReturnsCreatedOrder()
{
    // Arrange
    var request = new CreateOrderRequest
    {
        CustomerId = Guid.NewGuid(),
        ShippingAddressId = Guid.NewGuid(),
        Items = [new CreateOrderItemRequest { ProductId = Guid.NewGuid(), Quantity = 2 }]
    };
    var expectedOrder = OrderFaker.Build(request.CustomerId);
    _customerRepositoryMock.Setup(r => r.GetByIdAsync(request.CustomerId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(CustomerFaker.Build(request.CustomerId));
    _orderRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    // Act
    var result = await _sut.CreateOrderAsync(request, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.CustomerId.Should().Be(request.CustomerId);
    _orderRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

---

## 4. xUnit Attributes

| Attribute | Use Case |
|---|---|
| `[Fact]` | Single-case test with no parameters |
| `[Theory]` | Parameterised test — requires at least one `[InlineData]` or `[MemberData]` |
| `[InlineData(...)]` | Inline primitive arguments for `[Theory]` |
| `[MemberData(nameof(...))]` | Complex objects as theory data via static property |
| `[ClassData(typeof(...))]` | Separate data class for large datasets |
| `[Collection("...")]` | Group tests sharing a fixture or requiring sequential execution |
| `[Trait("Category", "Unit")]` | Categorise tests for selective run |

```csharp
// Theory example
[Theory]
[InlineData(0)]
[InlineData(-1)]
[InlineData(-100)]
public async Task CreateOrderAsync_WithZeroOrNegativeQuantity_ThrowsValidationException(int quantity)
{
    // Arrange
    var request = RequestFaker.CreateOrder() with { Quantity = quantity };

    // Act
    var act = async () => await _sut.CreateOrderAsync(request, CancellationToken.None);

    // Assert
    await act.Should().ThrowAsync<ValidationException>()
        .WithMessage("*quantity*");
}
```

---

## 5. Moq Usage

### Setup patterns

```csharp
// Return a value
_repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
    .ReturnsAsync(order);

// Return null (not found)
_repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
    .ReturnsAsync((Order?)null);

// Throw an exception
_repositoryMock.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
    .ThrowsAsync(new DbUpdateException("Unique constraint violated"));

// Capture argument
Order? capturedOrder = null;
_repositoryMock.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
    .Callback<Order, CancellationToken>((o, _) => capturedOrder = o)
    .Returns(Task.CompletedTask);
```

### Verify patterns

```csharp
// Called exactly once
_repositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);

// Called with specific argument
_repositoryMock.Verify(r => r.GetByIdAsync(expectedId, It.IsAny<CancellationToken>()), Times.Once);

// Never called
_repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
```

### Rules
- Always `new Mock<T>()` in the test constructor or in the test method — never share mocks across tests
- Use `It.IsAny<CancellationToken>()` for cancellation token arguments unless testing cancellation behaviour
- Prefer `MockBehavior.Strict` for dependencies whose unexpected calls indicate bugs
- Use `MockBehavior.Loose` only for dependencies that have many incidental call patterns (e.g., `ILogger<T>`)

---

## 6. FluentAssertions

### Basic assertions

```csharp
result.Should().NotBeNull();
result.Id.Should().Be(expectedId);
result.Status.Should().Be(OrderStatus.Pending);
result.Items.Should().HaveCount(2);
result.TotalAmount.Should().BeApproximately(99.99m, precision: 0.01m);
result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
```

### Collection assertions

```csharp
result.Items.Should().NotBeEmpty();
result.Items.Should().HaveCount(3);
result.Items.Should().AllSatisfy(item => item.Quantity.Should().BePositive());
result.Items.Should().Contain(item => item.ProductId == productId);
```

### Equivalence (preferred for DTOs)

```csharp
result.Should().BeEquivalentTo(expected, options =>
    options.ExcludingMissingMembers()
           .Excluding(r => r.CreatedAt));
```

### Exception assertions

```csharp
// Synchronous
var act = () => _sut.SomeMethod(invalidInput);
act.Should().Throw<ArgumentException>().WithMessage("*must not be null*");

// Asynchronous
var act = async () => await _sut.CreateOrderAsync(request, CancellationToken.None);
await act.Should().ThrowAsync<NotFoundException>()
    .WithMessage($"Order with ID {id} was not found.");
```

---

## 7. Service Unit Tests

- Mock all dependencies injected into the service
- Create the System Under Test (`_sut`) in the constructor or via a builder method
- Test every branch of every public method

```csharp
public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<ICustomerRepository> _customerRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<ILogger<OrderService>> _loggerMock = new();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService(
            _orderRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WhenOrderNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var act = async () => await _sut.GetOrderByIdAsync(id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{id}*");
    }
}
```

---

## 8. Controller Unit Tests

Mock the service layer; test the controller's HTTP contract (status codes, response shapes, routing to service methods):

```csharp
public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _orderServiceMock = new(MockBehavior.Strict);
    private readonly OrdersController _sut;

    public OrdersControllerTests()
    {
        _sut = new OrdersController(_orderServiceMock.Object, NullLogger<OrdersController>.Instance);
    }

    [Fact]
    public async Task CreateOrder_WithValidRequest_Returns201WithCreatedOrder()
    {
        // Arrange
        var request = RequestFaker.CreateOrder();
        var response = ResponseFaker.OrderResponse();
        _orderServiceMock.Setup(s => s.CreateOrderAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.CreateOrder(request, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdResult.Value.Should().BeEquivalentTo(response);
    }
}
```

---

## 9. Validator Tests

Test all validation rules in isolation:

```csharp
public class CreateOrderRequestValidatorTests
{
    private readonly CreateOrderRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_PassesValidation()
    {
        var request = RequestFaker.CreateOrder();
        var result = _validator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyItems_FailsWithExpectedMessage()
    {
        var request = RequestFaker.CreateOrder() with { Items = [] };
        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(CreateOrderRequest.Items) &&
            e.ErrorMessage.Contains("at least one item"));
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void Validate_WithEmptyGuid_FailsCustomerIdValidation(string guidString)
    {
        var request = RequestFaker.CreateOrder() with { CustomerId = Guid.Parse(guidString) };
        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateOrderRequest.CustomerId));
    }
}
```

---

## 10. Integration Tests (WebApplicationFactory)

Use `WebApplicationFactory<Program>` to spin up the full application in-memory:

```csharp
public class OrdersIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrdersIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_Orders_WithValidPayload_Returns201()
    {
        // Arrange
        var request = RequestFaker.CreateOrder();
        var token = JwtFaker.GenerateToken(userId: Guid.NewGuid());
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        body.Should().NotBeNull();
        body!.CustomerId.Should().Be(request.CustomerId);
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace real DbContext with in-memory or test container
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));
        });
    }
}
```

---

## 11. Test Data Builders (Fakers)

Use static faker classes to build consistent test data — no magic values inline in tests:

```csharp
public static class RequestFaker
{
    public static CreateOrderRequest CreateOrder(Guid? customerId = null) =>
        new()
        {
            CustomerId = customerId ?? Guid.NewGuid(),
            ShippingAddressId = Guid.NewGuid(),
            Items = [new CreateOrderItemRequest { ProductId = Guid.NewGuid(), Quantity = 1 }]
        };
}

public static class OrderFaker
{
    public static Order Build(Guid? customerId = null)
    {
        var order = new Order(customerId ?? Guid.NewGuid(), Guid.NewGuid());
        order.AddItem(Guid.NewGuid(), quantity: 1, unitPrice: 9.99m);
        return order;
    }
}
```

Optional: use the `Bogus` NuGet package for richer fake data generation.

---

## 12. Coverage Requirements

| Code Type | Minimum Line Coverage | Minimum Branch Coverage |
|---|---|---|
| Service layer | 90% | 85% |
| Repository layer | 85% | 80% |
| Controller layer | 85% | 80% |
| Validators | 100% | 100% |
| Domain entities / value objects | 90% | 85% |
| Middleware / exception handlers | 85% | 80% |
| Auth-critical / payment flows | 95% | 95% |

Run coverage:
```
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

---

## 13. Test Quality Checklist

Before marking tests complete, verify every item:

- [ ] Every test method follows `MethodName_StateUnderTest_ExpectedBehavior` naming
- [ ] Every test follows the Arrange-Act-Assert structure with section comments
- [ ] No test depends on execution order or shared mutable state
- [ ] No test hits a real external service (HTTP, email, SMS) — all mocked or intercepted
- [ ] No `Thread.Sleep` or `Task.Delay` in tests — use time abstraction (`IDateTimeProvider`)
- [ ] Each unit test completes in under 200 ms
- [ ] All mock setups are in `// Arrange` — no setup in `// Act` or `// Assert`
- [ ] All mock verifications are in `// Assert`
- [ ] FluentAssertions used consistently — no bare `Assert.Equal` / `Assert.True`
- [ ] No `try/catch` blocks inside test methods — let xUnit capture the exception
- [ ] No `if` / `for` / `while` logic inside test methods — use `[Theory]` with `[InlineData]`
- [ ] Test data created via faker methods — no inline magic values (GUIDs, strings, numbers)
- [ ] Each integration test database is isolated (fresh in-memory DB or container per test run)
- [ ] Coverage thresholds met for all changed modules

---

## 14. Test Configuration (`xunit.runner.json`)

```json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
  "parallelizeTestCollections": true,
  "maxParallelThreads": 4,
  "diagnosticMessages": false
}
```

Parallel test execution is enabled by default in xUnit. Ensure tests do not share static state.

---

## 15. Common Anti-Patterns to Avoid

| Anti-Pattern | Correct Approach |
|---|---|
| Mocking `HttpClient` directly | Use `IHttpClientFactory` with a typed client; test via `MockHttpMessageHandler` |
| Testing private methods | Test through public API; extract to a separate class if private logic is complex |
| `Assert.True(result != null)` | `result.Should().NotBeNull()` |
| Single test method testing multiple scenarios | Split into separate `[Fact]` or use `[Theory]` |
| Asserting on `exception.Message` string equality | `.WithMessage("*keyword*")` with wildcards |
| Using `Task.Run` to test async code | Mark test method `async Task` and use `await` directly |
| Hardcoding `Guid.Parse("000...")` in tests | `Guid.NewGuid()` or faker |
| Calling `SaveChangesAsync` in repository tests | Test repositories via integration tests against a real DB, not mocked DbContext |
