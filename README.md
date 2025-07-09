# Gomotel - Motel Booking System

A comprehensive backend API for motel booking and management built with .NET 9, implementing Domain-Driven Design (DDD) principles.

## 📋 Table of Contents

1. [Architecture Overview](#-architecture-overview)
2. [Detailed Layer Documentation](#-detailed-layer-documentation)
3. [Data Flow & Request Processing](#-data-flow--request-processing)
4. [Configuration & Dependency Injection](#-configuration--dependency-injection)
5. [Security Architecture](#-security-architecture)
6. [Database Architecture](#-database-architecture)
7. [Testing Strategy](#-testing-strategy)
8. [Development Workflow](#-development-workflow)
9. [Deployment & Production](#-deployment--production)
10. [API Documentation](#-api-documentation)
11. [Getting Started](#-getting-started)

## 🏗️ Architecture Overview

This project implements a **Clean Architecture** with **Domain-Driven Design (DDD)** principles, following the **Onion Architecture** pattern. The architecture is structured in layers with clear separation of concerns and dependency inversion.

### High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                            │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐        │
│  │ Controllers │  │ Middleware  │  │ Auth/JWT    │        │
│  └─────────────┘  └─────────────┘  └─────────────┘        │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐        │
│  │   Services  │  │  Validation │  │   MediatR   │        │
│  │  (App Logic)│  │ (Fluent)    │  │   (CQRS)    │        │
│  └─────────────┘  └─────────────┘  └─────────────┘        │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                       Domain Layer                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐        │
│  │  Entities   │  │ Domain      │  │ Value       │        │
│  │ (Aggregates)│  │ Services    │  │ Objects     │        │
│  └─────────────┘  └─────────────┘  └─────────────┘        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐        │
│  │ Repository  │  │ Domain      │  │ Exceptions  │        │
│  │ Interfaces  │  │ Events      │  │ (Business)  │        │
│  └─────────────┘  └─────────────┘  └─────────────┘        │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                   Infrastructure Layer                      │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐        │
│  │ Entity      │  │ Repository  │  │ Identity    │        │
│  │ Framework   │  │ Impl.       │  │ Provider    │        │
│  └─────────────┘  └─────────────┘  └─────────────┘        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐        │
│  │ Database    │  │ External    │  │ Caching     │        │
│  │ Context     │  │ Services    │  │ & Logging   │        │
│  └─────────────┘  └─────────────┘  └─────────────┘        │
└─────────────────────────────────────────────────────────────┘
```

### Core Principles

- **Dependency Inversion**: Higher-level modules do not depend on lower-level modules
- **Single Responsibility**: Each layer has a single, well-defined responsibility
- **Open/Closed Principle**: Open for extension, closed for modification
- **Interface Segregation**: Clients depend only on interfaces they use
- **Domain-Driven Design**: Business logic is centralized in the domain layer

## 🏛️ Detailed Layer Documentation

### 1. Domain Layer (`/Domain`)

**Responsibility**: Contains all business logic, rules, and domain concepts. This is the heart of the application.

#### Domain Structure

```
Domain/
├── Common/
│   ├── AggregateRoot.cs        # Base class for aggregates
│   └── BaseEntity.cs           # Base entity with common properties
├── Entities/
│   ├── Motel.cs               # Motel aggregate root
│   ├── Room.cs                # Room entity
│   ├── Reservation.cs         # Reservation aggregate root
│   └── Payment.cs             # Payment entity
├── ValueObjects/
│   ├── Address.cs             # Address value object
│   ├── Money.cs               # Money value object
│   └── TimeRange.cs           # Time range value object
├── Enums/
│   ├── ReservationStatus.cs   # Reservation status enum
│   ├── PaymentStatus.cs       # Payment status enum
│   └── RoomType.cs            # Room type enum
├── Events/
│   ├── IDomainEvent.cs        # Domain event interface
│   ├── ReservationCreatedEvent.cs
│   ├── PaymentApprovedEvent.cs
│   └── ReservationCancelledEvent.cs
├── Exceptions/
│   ├── DomainException.cs     # Base domain exception
│   ├── EntityNotFoundException.cs
│   ├── BusinessRuleException.cs
│   ├── ReservationException.cs
│   └── PaymentException.cs
├── Services/
│   ├── MotelDomainService.cs  # Motel business logic
│   ├── ReservationDomainService.cs
│   ├── RoomDomainService.cs
│   └── PaymentDomainService.cs
└── Repositories/
    ├── IMotelRepository.cs    # Repository interfaces
    ├── IReservationRepository.cs
    ├── IRoomRepository.cs
    └── IPaymentRepository.cs
```

#### Key Domain Concepts

**Aggregates**: Self-contained business objects that ensure consistency
- `Motel`: Manages rooms and motel information
- `Reservation`: Handles booking lifecycle and business rules

**Value Objects**: Immutable objects representing domain concepts
- `Address`: Complete address information
- `Money`: Amount with currency
- `TimeRange`: Start/end time with validation

**Domain Services**: Complex business logic that doesn't belong to a single entity
- `ReservationDomainService`: Handles reservation business rules
- `MotelDomainService`: Manages motel operations
- `RoomDomainService`: Room availability and pricing logic

**Domain Events**: Represent significant business events
- `ReservationCreatedEvent`: Triggered when a reservation is created
- `PaymentApprovedEvent`: Triggered when payment is approved
- `ReservationCancelledEvent`: Triggered when reservation is cancelled

### 2. Infrastructure Layer (`/Infrastructure`)

**Responsibility**: Implements external concerns like data persistence, external services, and framework-specific implementations.

#### Infrastructure Structure

```
Infrastructure/
├── Data/
│   ├── ApplicationDbContext.cs      # EF Core DbContext
│   ├── ApplicationDbContextFactory.cs # Design-time factory
│   └── SeedData.cs                  # Database seeding
├── Identity/
│   └── ApplicationUser.cs           # Identity user extension
├── Middleware/
│   └── GlobalExceptionMiddleware.cs # Global exception handling
└── Repositories/
    ├── MotelRepository.cs           # Motel repository implementation
    ├── ReservationRepository.cs     # Reservation repository
    ├── RoomRepository.cs            # Room repository
    └── PaymentRepository.cs         # Payment repository
```

#### Key Infrastructure Components

**Entity Framework Core Configuration**:
- Value object mapping using `OwnsOne()`
- Relationship configuration with proper cascade behaviors
- Index configuration for performance
- Migration management

**Repository Pattern Implementation**:
- Generic repository functionality
- Async operations with cancellation tokens
- Complex query support (overlapping reservations, availability checks)
- Proper error handling and logging

**Middleware Components**:
- Global exception handling with structured error responses
- Authentication/authorization middleware
- CORS configuration
- Request logging and monitoring

### 3. API Layer (`/Controllers`)

**Responsibility**: Handles HTTP requests, input validation, and response formatting.

#### API Structure

```
Controllers/
├── AuthController.cs           # Authentication endpoints
├── MotelsController.cs         # Motel management
├── RoomsController.cs          # Room operations
└── ReservationsController.cs   # Reservation management
```

#### API Layer Features

**Authentication & Authorization**:
- JWT token-based authentication
- Role-based authorization (Admin, MotelAdmin, User)
- Secure token generation and validation
- User management and profile endpoints

**API Versioning**:
- Version-aware routing
- Backward compatibility support
- Swagger documentation per version

**Input Validation**:
- FluentValidation integration
- Model binding with automatic validation
- Custom validation rules for domain concepts

**Error Handling**:
- Standardized error responses
- HTTP status code mapping
- Detailed error information for development
- Sanitized error messages for production

### 4. Application Layer (Mixed in `/Domain/Services`)

**Responsibility**: Orchestrates domain services and handles application-specific logic.

#### Application Services

**Service Pattern**:
- `IMotelService` / `MotelService`: Application-level motel operations
- `IReservationService` / `ReservationService`: Reservation workflow orchestration
- Coordinates between domain services and repositories
- Handles cross-cutting concerns (logging, validation, transactions)

**CQRS Ready**:
- MediatR integration for command/query separation
- Prepared for future CQRS implementation
- Command and query handlers can be easily added

## 🔄 Data Flow & Request Processing

### Request Processing Pipeline

```
1. HTTP Request → Controller
2. Controller → Input Validation (FluentValidation)
3. Controller → Application Service
4. Application Service → Domain Service(s)
5. Domain Service → Repository Interface
6. Repository → Database (EF Core)
7. Response flows back through the chain
```

### Example: Create Reservation Flow

```csharp
// 1. HTTP POST /api/reservations
[HttpPost]
public async Task<ActionResult<ReservationDto>> CreateReservation(CreateReservationRequest request)
{
    // 2. Input validation (automatic via FluentValidation)
    
    // 3. Call application service
    var reservation = await _reservationService.CreateReservationAsync(
        request.MotelId, 
        request.RoomId, 
        GetCurrentUserId(), 
        request.StartTime, 
        request.EndTime, 
        request.SpecialRequests
    );
    
    // 4. Map to DTO and return
    return Ok(_mapper.Map<ReservationDto>(reservation));
}
```

```csharp
// Application Service Layer
public async Task<Reservation> CreateReservationAsync(...)
{
    // Business validation
    var motel = await _motelRepository.GetByIdAsync(motelId);
    // ... validation logic
    
    // Domain service for business logic
    var totalAmount = _reservationDomainService.CalculateTotalAmount(room, timeRange);
    var reservation = _reservationDomainService.CreateReservation(...);
    
    // Persistence
    return await _reservationRepository.AddAsync(reservation);
}
```

### Error Handling Flow

```
1. Exception occurs in any layer
2. GlobalExceptionMiddleware catches exception
3. Exception type mapped to appropriate HTTP status
4. Structured error response returned to client
5. Logging performed based on severity
```

## ⚙️ Configuration & Dependency Injection

### Service Registration (Program.cs)

```csharp
// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsDevelopment() && 
        builder.Configuration.GetValue<bool>("UseInMemoryDatabase"))
    {
        options.UseInMemoryDatabase("GomotelDb");
    }
    else
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});

// Identity & Authentication
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// JWT Configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* JWT configuration */ });

// Repository Pattern
builder.Services.AddScoped<IMotelRepository, MotelRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();

// Domain Services
builder.Services.AddScoped<MotelDomainService>();
builder.Services.AddScoped<ReservationDomainService>();

// Application Services
builder.Services.AddScoped<IMotelService, MotelService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
```

### Configuration Structure

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=GomotelDb;..."
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key",
    "Issuer": "gomotel-api",
    "Audience": "gomotel-client",
    "ExpiryMinutes": 60
  },
  "UseInMemoryDatabase": true,
  "SeedDatabase": true,
  "AllowedOrigins": ["http://localhost:3000"]
}
```

### Middleware Pipeline

```csharp
// Development
app.UseSwagger();
app.UseSwaggerUI();

// Production
app.UseHttpsRedirection();

// Custom Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Framework Middleware
app.UseCors("AllowedOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

## 🔐 Security Architecture

### Authentication Flow

```
1. User sends login credentials
2. AuthController validates credentials
3. JWT token generated with user claims
4. Token returned to client
5. Client includes token in Authorization header
6. JWT middleware validates token on each request
7. User principal populated for authorization
```

### Authorization Levels

```csharp
// Role-based Policies
[Authorize(Roles = "Admin")]          // Full system access
[Authorize(Roles = "MotelAdmin")]     // Motel-specific access
[Authorize(Roles = "User")]           // Basic user access

// Policy-based Authorization
[Authorize(Policy = "MotelOwner")]    // Custom policy
```

### Security Features

- **JWT Token Security**: Symmetric key encryption, configurable expiry
- **Role-Based Access Control**: Three-tier role system
- **HTTPS Enforcement**: Configurable SSL/TLS in production
- **CORS Configuration**: Configurable allowed origins
- **Input Validation**: Comprehensive validation using FluentValidation
- **SQL Injection Prevention**: Entity Framework Core parameterized queries
- **Exception Sanitization**: Sensitive information filtered in production

## 🗄️ Database Architecture

### Entity Framework Core Configuration

**Database Providers**:
- **Development**: In-Memory Database (for testing)
- **Production**: SQL Server
- **Alternative**: SQLite support included

**Migration Strategy**:
```bash
# Create new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Generate SQL script
dotnet ef migrations script
```

### Value Object Mapping

```csharp
// Address Value Object
entity.OwnsOne(e => e.Address, address =>
{
    address.Property(a => a.Street).IsRequired().HasMaxLength(200);
    address.Property(a => a.City).IsRequired().HasMaxLength(100);
    // ... other properties
});

// Money Value Object
entity.OwnsOne(e => e.PricePerHour, money =>
{
    money.Property(m => m.Amount).HasColumnType("decimal(18,2)");
    money.Property(m => m.Currency).HasMaxLength(3);
});
```

### Database Relationships

```
Motel (1) ←→ (N) Room
Room (1) ←→ (N) Reservation
Reservation (1) ←→ (1) Payment
ApplicationUser (1) ←→ (N) Reservation
```

### Indexing Strategy

```csharp
// Performance indexes
builder.Entity<Motel>().HasIndex(e => e.OwnerId);
builder.Entity<Room>().HasIndex(e => new { e.MotelId, e.RoomNumber }).IsUnique();
builder.Entity<Reservation>().HasIndex(e => new { e.RoomId, e.Status });
```

## 🧪 Testing Strategy

### Current Testing Implementation

```csharp
// Domain Tests (Tests/DomainTests.cs)
public static void RunTests()
{
    // Value Object Tests
    TestAddressCreation();
    TestMoneyOperations();
    TestTimeRangeOperations();
    
    // Note: Entity tests require domain service refactoring
}
```

### Recommended Testing Approach

#### Unit Tests

```csharp
// Domain Service Tests
[Test]
public void CreateReservation_ValidInput_ReturnsReservation()
{
    // Arrange
    var domainService = new ReservationDomainService();
    var timeRange = TimeRange.Create(DateTime.Now.AddHours(1), DateTime.Now.AddHours(3));
    
    // Act
    var reservation = domainService.CreateReservation(/*...*/);
    
    // Assert
    Assert.That(reservation.Status, Is.EqualTo(ReservationStatus.Pending));
}

// Repository Tests
[Test]
public async Task GetByIdAsync_ExistingId_ReturnsEntity()
{
    // Arrange
    using var context = CreateInMemoryContext();
    var repository = new MotelRepository(context);
    
    // Act & Assert
    var motel = await repository.GetByIdAsync(existingId);
    Assert.That(motel, Is.Not.Null);
}
```

#### Integration Tests

```csharp
// Controller Integration Tests
[Test]
public async Task CreateReservation_ValidRequest_Returns201()
{
    // Arrange
    var client = _factory.CreateClient();
    var request = new CreateReservationRequest { /* ... */ };
    
    // Act
    var response = await client.PostAsJsonAsync("/api/reservations", request);
    
    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
}
```

### Testing Tools & Frameworks

- **NUnit**: Unit testing framework
- **Moq/NSubstitute**: Mocking framework
- **Microsoft.AspNetCore.Mvc.Testing**: Integration testing
- **EF Core InMemory**: Database testing

## 💻 Development Workflow

### Project Structure

```
gomotel/
├── Controllers/          # API controllers
├── Domain/              # Domain layer (business logic)
│   ├── Common/          # Base classes
│   ├── Entities/        # Domain entities
│   ├── ValueObjects/    # Value objects
│   ├── Services/        # Domain services
│   ├── Events/          # Domain events
│   ├── Exceptions/      # Domain exceptions
│   └── Repositories/    # Repository interfaces
├── Infrastructure/      # Infrastructure layer
│   ├── Data/           # Entity Framework
│   ├── Identity/       # Identity extensions
│   ├── Middleware/     # Custom middleware
│   └── Repositories/   # Repository implementations
├── Tests/              # Test projects
├── Migrations/         # EF Core migrations
├── Properties/         # Launch settings
├── Program.cs          # Application startup
└── appsettings.json    # Configuration
```

### Development Commands

```bash
# Development
dotnet run                    # Start application
dotnet watch run             # Start with hot reload
dotnet test                  # Run tests (domain logic)

# Database
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet ef migrations script

# Package Management
dotnet add package PackageName
dotnet restore
dotnet build
```

### Adding New Features

1. **Add Domain Entity/Value Object**
2. **Create Domain Service** (if needed)
3. **Add Repository Interface** (Domain layer)
4. **Implement Repository** (Infrastructure layer)
5. **Create Application Service** (if needed)
6. **Add Controller** with proper validation
7. **Add Tests** for all layers
8. **Update Database** (migration if needed)

## 🚀 Deployment & Production

### Production Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=GomotelDb;..."
  },
  "JwtSettings": {
    "SecretKey": "production-secret-key",
    "ExpiryMinutes": 15
  },
  "UseInMemoryDatabase": false,
  "UseHttpsRedirection": true,
  "SeedDatabase": false,
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Logging Configuration

```csharp
// Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/gomotel-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

### Production Considerations

- **HTTPS**: Enforced in production
- **Database**: SQL Server with proper connection pooling
- **Logging**: Structured logging with Serilog
- **Error Handling**: Sanitized error responses
- **Security**: Production-grade JWT secrets
- **Performance**: Connection pooling, caching strategies
- **Monitoring**: Application insights integration ready

### Docker Support (Future)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["gomotel.csproj", "."]
RUN dotnet restore "gomotel.csproj"
COPY . .
RUN dotnet build "gomotel.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "gomotel.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "gomotel.dll"]
```

## 📚 API Documentation

### Swagger/OpenAPI Integration

The API includes comprehensive Swagger documentation available at:
- **Development**: `http://localhost:5140/swagger`
- **Production**: `https://yourdomain.com/swagger`

### API Versioning

```csharp
// Version 1.0 (default)
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class MotelsController : ControllerBase
```

### Authentication in Swagger

JWT Bearer token authentication is configured in Swagger UI:
1. Click "Authorize" button
2. Enter: `Bearer {your-jwt-token}`
3. All requests will include authentication header

## 🚀 Features

### Core Functionality
- **Motel Management**: CRUD operations for motels
- **Room Management**: Room types, pricing, availability
- **Reservation System**: Time-based booking with conflict detection
- **Payment Processing**: Payment status tracking
- **User Roles**: Admin, MotelAdmin, User

### Technical Features
- **Domain Events**: Event-driven architecture
- **CQRS Ready**: MediatR integration
- **Validation**: FluentValidation support
- **Logging**: Structured logging with Serilog
- **Testing**: Domain logic testing

## 🛠️ Technology Stack

- **.NET 9** - Latest .NET framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM
- **SQL Server** - Primary database
- **JWT Bearer** - Authentication
- **Swagger/OpenAPI** - API documentation
- **Serilog** - Structured logging
- **MediatR** - CQRS pattern
- **FluentValidation** - Input validation

## 📋 Prerequisites

- .NET 9 SDK
- SQL Server Express (or SQL Server)
- Visual Studio 2022 or VS Code

## 🚀 Getting Started

### 1. Clone the Repository
```bash
git clone <repository-url>
cd gomotel
```

### 2. Configuration
Update `appsettings.json` with your database connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=GomotelDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

### 3. Database Setup
```bash
# Create migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

### 4. Run the Application
```bash
# Run in development mode (uses in-memory database)
dotnet run

# Run domain tests
dotnet run test
```

### 5. Access the API
- **API Base URL**: `http://localhost:5140`
- **Swagger UI**: `http://localhost:5140/swagger`
- **HTTPS**: `https://localhost:7245`

## 📊 API Endpoints

### Motels
- `GET /api/v1/motels` - Get all motels
- `GET /api/v1/motels/{id}` - Get motel by ID
- `POST /api/v1/motels` - Create motel (Admin/MotelAdmin)
- `PUT /api/v1/motels/{id}` - Update motel (Admin/MotelAdmin)
- `DELETE /api/v1/motels/{id}` - Delete motel (Admin only)

### Authentication
- JWT-based authentication
- Role-based authorization (Admin, MotelAdmin, User)

## 🏛️ Domain Model

### Motel Aggregate
```csharp
public class Motel : AggregateRoot
{
    public string Name { get; private set; }
    public Address Address { get; private set; }
    public string PhoneNumber { get; private set; }
    public string Email { get; private set; }
    public Guid OwnerId { get; private set; }
    public IReadOnlyCollection<Room> Rooms { get; }
}
```

### Room Entity
```csharp
public class Room : BaseEntity
{
    public string RoomNumber { get; private set; }
    public RoomType Type { get; private set; }
    public Money PricePerHour { get; private set; }
    public bool IsAvailable { get; private set; }
}
```

### Reservation Aggregate
```csharp
public class Reservation : AggregateRoot
{
    public TimeRange TimeRange { get; private set; }
    public ReservationStatus Status { get; private set; }
    public Money TotalAmount { get; private set; }
    public string? SpecialRequests { get; private set; }
}
```

## 🔒 Security

- **JWT Authentication**: Secure token-based authentication
- **Role-based Authorization**: Three user roles with different permissions
- **HTTPS**: SSL/TLS encryption
- **Input Validation**: Comprehensive validation using FluentValidation

## 📝 Business Rules

### Reservation Rules
- No overlapping reservations for the same room
- Reservations must be in the future
- Payment must be approved before confirmation
- Only confirmed reservations can be checked in

### Motel Rules
- Each motel has a unique owner (MotelAdmin)
- Room numbers must be unique within a motel
- Only active motels are shown in public listings

## 🧪 Testing

### Domain Tests
Run the domain logic tests to verify business rules:
```bash
dotnet run test
```

### Test Coverage
- Value Object validation
- Entity business logic
- Domain event generation
- Repository operations

## 📈 Development Roadmap

### Phase 1 (Current)
- [x] Domain model implementation
- [x] Basic API endpoints
- [x] Authentication & authorization
- [x] Database setup

### Phase 2 (Next)
- [ ] Complete CRUD operations for all entities
- [ ] Payment integration
- [ ] Email notifications
- [ ] Advanced search and filtering

### Phase 3 (Future)
- [ ] Real-time notifications
- [ ] Mobile app support
- [ ] Analytics and reporting
- [ ] Multi-language support

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📄 License

This project is licensed under the MIT License.

## 👥 Team

- **Backend Developer**: Senior .NET Developer
- **Architecture**: Domain-Driven Design
- **Database**: SQL Server with Entity Framework Core

---

**Note**: This is a demonstration project showcasing modern .NET development practices with DDD, CQRS, and clean architecture principles. 