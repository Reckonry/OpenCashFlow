# Tech Stack Patterns - Gestione Incassi

**Framework**: ASP.NET Core / .NET 9
**Updated**: 2025-10-13

---

## ASP.NET Core Best Practices

### Dependency Injection

**Registration Patterns**:

```csharp
// Scoped - Per HTTP request (EF DbContext, services with state)
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ApplicationDbContext>();

// Transient - New instance each time (stateless services)
builder.Services.AddTransient<IEmailService, EmailService>();

// Singleton - One instance for app lifetime (caching, configuration)
builder.Services.AddSingleton<ICacheService, CacheService>();
```

**When to Use**:
- **Scoped**: Services that need DbContext, user context from HttpContext
- **Transient**: Lightweight services with no state
- **Singleton**: Heavy initialization, shared state (use with caution)

### Controller Best Practices

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication by default
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentService paymentService,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Payment_Detail_DTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Payment_Detail_DTO>> GetPayment(Guid id)
    {
        try
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            return Ok(payment);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Payment not found: {PaymentId}", id);
            return NotFound(new { message = ex.Message });
        }
    }
}
```

**Key Patterns**:
- `[ApiController]` - Automatic model validation, binding source inference
- `[ProducesResponseType]` - OpenAPI documentation
- Try-catch for specific exceptions only (global handler catches rest)
- Return typed `ActionResult<T>` for API endpoints

---

## Entity Framework Core Patterns

### DbContext Configuration

**Connection String**:
```csharp
// Priority: Environment variable > appsettings.json
var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONN_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnectionString");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
);
```

### Repository Pattern

**Interface**:
```csharp
public interface IPaymentRepository
{
    Task<Payment> GetByIdAsync(Guid id, Guid companyId);
    Task<List<Payment>> GetAllAsync(Guid companyId);
    Task<Payment> CreateAsync(Payment payment);
    Task<Payment> UpdateAsync(Payment payment);
    Task DeleteAsync(Guid id, Guid companyId);
}
```

**Implementation**:
```csharp
public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Payment> GetByIdAsync(Guid id, Guid companyId)
    {
        return await _context.Payments
            .Include(p => p.PaymentMethod)
            .Include(p => p.DocumentType)
            .Where(p => p.GICompanyID == companyId && !p.IsDeleted)
            .FirstOrDefaultAsync(p => p.GIPaymentID == id);
    }
}
```

**Key Points**:
- Always filter by `companyId` (multi-tenancy)
- Always filter `!IsDeleted` (soft delete)
- Use `Include()` for eager loading relationships
- Async all the way

### Query Optimization

**Good - Eager Loading**:
```csharp
var payments = await _context.Payments
    .Include(p => p.PaymentMethod)
    .Include(p => p.DocumentType)
    .Where(p => p.GICompanyID == companyId)
    .ToListAsync();
```

**Bad - N+1 Query Problem**:
```csharp
var payments = await _context.Payments
    .Where(p => p.GICompanyID == companyId)
    .ToListAsync();

// This causes N additional queries!
foreach (var payment in payments)
{
    var method = payment.PaymentMethod; // Lazy load
}
```

**Projection for Performance**:
```csharp
// Load only needed fields
var paymentSummaries = await _context.Payments
    .Where(p => p.GICompanyID == companyId)
    .Select(p => new Payment_List_DTO
    {
        PaymentId = p.GIPaymentID,
        Amount = p.Amount,
        Date = p.Date,
        PaymentMethodName = p.PaymentMethod.Name
    })
    .ToListAsync();
```

### Transaction Management

**Standard Transaction**:
```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // Multiple operations
    await _context.Payments.AddAsync(payment);
    await _context.SaveChangesAsync();

    cashBalance.Balance += payment.Amount;
    await _context.SaveChangesAsync();

    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

**Optimistic Concurrency (Cash Balance)**:
```csharp
const int maxRetries = 3;
for (int i = 0; i < maxRetries; i++)
{
    try
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        var cashBalance = await _context.CashBalances
            .FirstOrDefaultAsync(cb => cb.CompanyId == companyId);

        // PostgreSQL xmin provides row version
        cashBalance.Balance += delta;

        await _context.CashLedger.AddAsync(new CashLedger
        {
            CompanyId = companyId,
            Delta = delta,
            RefType = "Payment",
            RefId = paymentId
        });

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        break; // Success
    }
    catch (DbUpdateConcurrencyException)
    {
        if (i == maxRetries - 1) throw;
        // Retry
        _context.ChangeTracker.Clear();
    }
}
```

### Migrations Best Practices

**Create Migration**:
```bash
dotnet ef migrations add DescriptiveName --project GestioneIncassi-Shared --startup-project GestioneIncassi-API
```

**Review Before Applying**:
- Always review generated migration code
- Check for data loss warnings
- Test on staging first

**Seed Data in Migration**:
```csharp
migrationBuilder.InsertData(
    table: "PaymentMethods",
    columns: new[] { "GIPaymentMethodID", "Name", "GICompanyID" },
    values: new object[] { Guid.NewGuid(), "Cash", companyId }
);
```

---

## AutoMapper Patterns

### Profile Configuration

```csharp
public class PaymentMappingProfile : Profile
{
    public PaymentMappingProfile()
    {
        // Entity to DTO
        CreateMap<Payment, Payment_Detail_DTO>()
            .ForMember(dest => dest.PaymentMethodName,
                opt => opt.MapFrom(src => src.PaymentMethod.Name));

        // DTO to Entity
        CreateMap<Payment_Create_DTO, Payment>()
            .ForMember(dest => dest.GIPaymentID, opt => opt.Ignore())
            .ForMember(dest => dest.DateIns, opt => opt.Ignore());
    }
}
```

**Best Practices**:
- One profile per entity domain
- Use `ForMember` for complex mappings
- Ignore auto-generated fields (IDs, timestamps)
- Test mappings in unit tests

### Usage in Services

```csharp
public class PaymentService
{
    private readonly IMapper _mapper;

    public async Task<Payment_Detail_DTO> GetPaymentAsync(Guid id)
    {
        var payment = await _repository.GetByIdAsync(id);
        return _mapper.Map<Payment_Detail_DTO>(payment);
    }

    public async Task<Payment> CreatePaymentAsync(Payment_Create_DTO dto)
    {
        var payment = _mapper.Map<Payment>(dto);
        // ... business logic
        return await _repository.CreateAsync(payment);
    }
}
```

---

## Authentication & Authorization Patterns

### JWT Configuration

**API `Program.cs`**:
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
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
        };

        // Read token from cookie
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["AuthToken"];
                return Task.CompletedTask;
            }
        };
    });
```

### Multi-Audience Support

```csharp
// appsettings.json can have string or array
"JwtSettings": {
  "Audience": ["App", "Admin", "API"]
}

// Load audiences
var audiences = builder.Configuration.GetSection("JwtSettings:Audience").Get<string[]>()
    ?? new[] { builder.Configuration["JwtSettings:Audience"] };

options.TokenValidationParameters.ValidAudiences = audiences;
```

### Extracting User Context

```csharp
public class PaymentService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private Guid GetCurrentCompanyId()
    {
        var companyIdClaim = _httpContextAccessor.HttpContext?.User
            .FindFirst("GICompanyID")?.Value;

        if (string.IsNullOrEmpty(companyIdClaim))
            throw new UnauthorizedAccessException("Company context not found");

        return Guid.Parse(companyIdClaim);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.Parse(userIdClaim);
    }
}
```

**Register HttpContextAccessor**:
```csharp
builder.Services.AddHttpContextAccessor();
```

---

## HttpClient Patterns (App/Admin Projects)

### Configuration

**Program.cs**:
```csharp
builder.Services.AddTransient<BearerTokenHandler>();

builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Account:API"]);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddHttpMessageHandler<BearerTokenHandler>();
```

### BearerTokenHandler

```csharp
public class BearerTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BearerTokenHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
```

### API Service Pattern

```csharp
public class PaymentAPIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentAPIService> _logger;

    public PaymentAPIService(
        IHttpClientFactory httpClientFactory,
        ILogger<PaymentAPIService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("API");
        _logger = logger;
    }

    public async Task<Payment_Detail_DTO> GetPaymentAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/payment/{id}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Payment_Detail_DTO>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching payment {PaymentId}", id);
            throw;
        }
    }

    public async Task<Payment> CreatePaymentAsync(Payment_Create_DTO dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/payment", dto);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Payment>();
    }
}
```

---

## SignalR Patterns

### Hub Implementation

```csharp
[Authorize]
public class PaymentHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var companyId = Context.User?.FindFirst("GICompanyID")?.Value;
        if (!string.IsNullOrEmpty(companyId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, companyId);
        }
        await base.OnConnectedAsync();
    }

    // Server-to-client methods defined on client
    public async Task NotifyPaymentCreated(Payment payment)
    {
        await Clients.Group(payment.GICompanyID.ToString())
            .SendAsync("PaymentCreated", payment);
    }
}
```

### Client (JavaScript)

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/paymentHub")
    .build();

connection.on("PaymentCreated", (payment) => {
    console.log("New payment:", payment);
    // Update UI
});

connection.start().catch(err => console.error(err));
```

### Service Usage

```csharp
public class PaymentService
{
    private readonly IHubContext<PaymentHub> _hubContext;

    public async Task CreatePaymentAsync(Payment_Create_DTO dto)
    {
        var payment = await _repository.CreateAsync(mapped);

        // Notify connected clients
        await _hubContext.Clients.Group(payment.GICompanyID.ToString())
            .SendAsync("PaymentCreated", payment);

        return payment;
    }
}
```

---

## Error Handling Patterns

### Global Exception Middleware

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception");

        var (statusCode, message) = exception switch
        {
            ArgumentException => (400, exception.Message),
            UnauthorizedAccessException => (403, "Forbidden"),
            KeyNotFoundException => (404, "Resource not found"),
            _ => (500, "Internal server error")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new { error = message };
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

**Registration**:
```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

### Service Layer Error Handling

```csharp
public async Task<Payment> CreatePaymentAsync(Payment_Create_DTO dto)
{
    // Validation
    if (dto.Amount <= 0)
        throw new ArgumentException("Amount must be positive", nameof(dto.Amount));

    // Business rule
    var method = await _context.PaymentMethods.FindAsync(dto.PaymentMethodId);
    if (method == null)
        throw new KeyNotFoundException($"Payment method {dto.PaymentMethodId} not found");

    // Proceed with creation
    var payment = _mapper.Map<Payment>(dto);
    return await _repository.CreateAsync(payment);
}
```

---

## Logging Patterns

### Serilog Configuration

**Program.cs**:
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .WriteTo.Console()
    .WriteTo.File("../Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

### Structured Logging

```csharp
public class PaymentService
{
    private readonly ILogger<PaymentService> _logger;

    public async Task CreatePaymentAsync(Payment_Create_DTO dto)
    {
        _logger.LogInformation(
            "Creating payment: Amount={Amount}, Method={MethodId}, Company={CompanyId}",
            dto.Amount, dto.PaymentMethodId, GetCurrentCompanyId());

        try
        {
            var payment = await _repository.CreateAsync(mapped);

            _logger.LogInformation(
                "Payment created successfully: Id={PaymentId}",
                payment.GIPaymentID);

            return payment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating payment for company {CompanyId}",
                GetCurrentCompanyId());
            throw;
        }
    }
}
```

**Log Levels**:
- `Trace` - Very detailed, typically only for diagnostics
- `Debug` - Internal system events
- `Information` - General informational messages
- `Warning` - Unexpected but not critical
- `Error` - Error events
- `Critical` - Critical failures

---

## Testing Patterns

### Unit Test Example

```csharp
public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly PaymentService _service;

    public PaymentServiceTests()
    {
        _mockRepo = new Mock<IPaymentRepository>();
        _mockMapper = new Mock<IMapper>();
        _service = new PaymentService(_mockRepo.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task CreatePayment_WithValidData_ReturnsPayment()
    {
        // Arrange
        var dto = new Payment_Create_DTO { Amount = 100 };
        var payment = new Payment { GIPaymentID = Guid.NewGuid() };

        _mockMapper.Setup(m => m.Map<Payment>(dto)).Returns(payment);
        _mockRepo.Setup(r => r.CreateAsync(payment)).ReturnsAsync(payment);

        // Act
        var result = await _service.CreatePaymentAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(payment.GIPaymentID, result.GIPaymentID);
    }
}
```

### Integration Test with In-Memory DB

```csharp
public class PaymentIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PaymentIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetPayment_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/payment/some-id");

        response.EnsureSuccessStatusCode();
    }
}
```

---

## Performance Optimization

### Caching Strategy

```csharp
// Use IMemoryCache for frequently accessed data
public class PaymentMethodService
{
    private readonly IMemoryCache _cache;
    private const string CacheKey = "PaymentMethods_{0}"; // {0} = companyId

    public async Task<List<PaymentMethod>> GetPaymentMethodsAsync(Guid companyId)
    {
        var key = string.Format(CacheKey, companyId);

        if (!_cache.TryGetValue(key, out List<PaymentMethod> methods))
        {
            methods = await _repository.GetAllAsync(companyId);

            _cache.Set(key, methods, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });
        }

        return methods;
    }
}
```

### Async Best Practices

**Good**:
```csharp
public async Task<List<Payment>> GetPaymentsAsync()
{
    return await _context.Payments.ToListAsync();
}
```

**Bad - Blocking async**:
```csharp
public List<Payment> GetPayments()
{
    return _context.Payments.ToListAsync().Result; // DON'T DO THIS
}
```

**Bad - Unnecessary async**:
```csharp
public async Task<int> AddNumbers(int a, int b)
{
    return await Task.FromResult(a + b); // Pointless async
}

// Better:
public int AddNumbers(int a, int b) => a + b;
```

---

## Common Mistakes to Avoid

### 1. Not Filtering by Company ID

```csharp
// BAD - Exposes all companies' data!
var payments = await _context.Payments.ToListAsync();

// GOOD
var payments = await _context.Payments
    .Where(p => p.GICompanyID == companyId)
    .ToListAsync();
```

### 2. Forgetting Soft Delete Filter

```csharp
// BAD - Shows deleted records
var payment = await _context.Payments.FindAsync(id);

// GOOD
var payment = await _context.Payments
    .Where(p => !p.IsDeleted)
    .FirstOrDefaultAsync(p => p.GIPaymentID == id);
```

### 3. N+1 Query Problem

```csharp
// BAD - Causes N queries
var payments = await _context.Payments.ToListAsync();
foreach (var p in payments)
{
    var method = p.PaymentMethod; // Lazy load
}

// GOOD
var payments = await _context.Payments
    .Include(p => p.PaymentMethod)
    .ToListAsync();
```

### 4. Not Using Transactions for Related Changes

```csharp
// BAD - Can leave inconsistent state
await _context.Payments.AddAsync(payment);
await _context.SaveChangesAsync();
cashBalance.Balance += payment.Amount;
await _context.SaveChangesAsync(); // If this fails, payment is saved but balance isn't updated

// GOOD
using var transaction = await _context.Database.BeginTransactionAsync();
await _context.Payments.AddAsync(payment);
cashBalance.Balance += payment.Amount;
await _context.SaveChangesAsync();
await transaction.CommitAsync();
```

---

## Framework-Specific Tools

### EF Core Tools

```bash
# Install globally
dotnet tool install --global dotnet-ef

# Update
dotnet tool update --global dotnet-ef
```

### Useful Commands

```bash
# Check connection
dotnet ef dbcontext info --project GestioneIncassi-Shared --startup-project GestioneIncassi-API

# List migrations
dotnet ef migrations list --project GestioneIncassi-Shared --startup-project GestioneIncassi-API

# Generate SQL script
dotnet ef migrations script --project GestioneIncassi-Shared --startup-project GestioneIncassi-API
```

---

**Last Updated**: 2025-10-13
