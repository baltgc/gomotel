# Gomotel - Motel Booking System

A comprehensive backend API for motel booking and management built with .NET 9, implementing Domain-Driven Design (DDD) principles.

## 🏗️ Architecture

This project follows **Domain-Driven Design (DDD)** principles with a clean architecture approach:

### Domain Layer
- **Entities**: Motel, Room, Reservation, Payment
- **Value Objects**: Address, Money, TimeRange
- **Enums**: ReservationStatus, PaymentStatus, RoomType
- **Domain Events**: ReservationCreated, PaymentApproved, ReservationCancelled
- **Repository Interfaces**: IMotelRepository, IReservationRepository

### Infrastructure Layer
- **Entity Framework Core** with SQL Server support
- **Repository Pattern** implementation
- **Identity Framework** for user management
- **In-Memory Database** support for development

### API Layer
- **RESTful API** with versioning
- **JWT Authentication** and role-based authorization
- **Swagger/OpenAPI** documentation
- **CORS** configuration

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