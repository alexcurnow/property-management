# Property Management System - Maintenance Operations

A learning project demonstrating **Domain-Driven Design (DDD)**, **CQRS**, **Event-Driven Architecture**, and **Vertical Slice Architecture** using .NET 9 and Blazor Interactive Server-Side Rendering.

## Architecture Overview

### Design Patterns & Principles

- **Domain-Driven Design (DDD)**: Rich domain models with encapsulated business logic
- **CQRS**: Command-Query separation using MediatR
- **Vertical Slice Architecture**: Features organized by business capability
- **Event-Driven Design**: Domain events for cross-aggregate communication
- **Repository Pattern**: Entity Framework Core with PostgreSQL

### Technology Stack

- **.NET 9** with C# 12
- **Blazor Interactive SSR** (Server-Side Rendering)
- **MudBlazor** for UI components
- **Fluxor** for state management and optimistic updates
- **MediatR** for CQRS implementation
- **FluentValidation** for input validation
- **Entity Framework Core 9** with PostgreSQL
- **Docker** for containerization

## Domain Model

### Bounded Context: Maintenance

#### Aggregates

1. **WorkOrder** - Central aggregate for maintenance requests
   - Status transitions: New → Assigned → InProgress → Completed
   - Priority levels: Low, Medium, High, Emergency
   - Business rules enforced through domain methods

2. **Property/Unit** - Location information
   - Properties contain multiple units
   - Address as value object

3. **Vendor/Technician** - Service providers
   - Specialization-based assignment
   - Availability tracking

4. **MaintenanceSchedule** - Preventive maintenance
   - Recurrence patterns
   - Auto-generation of work orders

#### Value Objects

- `Money` - Amount with currency
- `DateRange` - Scheduling windows
- `Address` - Physical location
- `WorkOrderStatus` - Valid state transitions
- `WorkOrderPriority` - Priority levels

#### Domain Services

- `WorkOrderSchedulingService` - Coordinates scheduling across aggregates

## Project Structure

```
PropertyManagement/
├── PropertyManagement.sln
├── docker-compose.yml
├── Dockerfile
├── src/
│   └── PropertyManagement.Web/
│       ├── Features/                    # Vertical slices by business capability
│       │   ├── MaintenanceRequests/
│       │   │   ├── Submit/             # Complete feature in one place
│       │   │   │   ├── SubmitMaintenanceRequest.cs    (Command, Handler, Validator)
│       │   │   │   └── SubmitMaintenanceRequest.razor (UI Component)
│       │   │   ├── Review/
│       │   │   └── Assign/
│       │   ├── PreventiveMaintenance/
│       │   └── VendorAssignments/
│       ├── Domain/                      # Rich domain models
│       │   ├── WorkOrders/
│       │   ├── Properties/
│       │   ├── Vendors/
│       │   ├── Scheduling/
│       │   ├── Common/
│       │   └── Services/
│       ├── Infrastructure/              # Technical concerns
│       │   ├── Persistence/
│       │   └── Behaviors/
│       └── Shared/                      # Cross-cutting UI components
│           ├── Components/Layout/
│           └── State/
```

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [PostgreSQL](https://www.postgresql.org/download/) (if running locally)

### Running with Docker (Recommended)

1. **Start the application and database:**

```bash
docker-compose up --build
```

2. **Access the application:**
   - Web UI: http://localhost:8080
   - PostgreSQL: localhost:5433

3. **Stop the application:**

```bash
docker-compose down
```

### Running Locally

1. **Start PostgreSQL** (ensure it's running on port 5432)

2. **Update connection string** in `appsettings.Development.json`

3. **Restore dependencies:**

```bash
dotnet restore
```

4. **Run the application:**

```bash
cd src/PropertyManagement.Web
dotnet run
```

5. **Access the application:** The console will show the URL (typically https://localhost:5001)

### Configuration Management

#### Environment Variables

For Docker deployments, you can customize environment variables:

1. **Copy the example file:**
```bash
cp .env.example .env
```

2. **Edit `.env` with your values:**
```env
POSTGRES_PASSWORD=your_secure_password
POSTGRES_PORT=5433
WEB_PORT=8080
```

3. **The `.env` file is ignored by git** to prevent committing secrets.

#### Application Settings

The project follows a layered configuration approach:

- `appsettings.json` - Base settings (committed to git)
- `appsettings.Development.json` - Development defaults (committed to git with safe values)
- `appsettings.Development.local.json` - Your local overrides (ignored by git)
- Environment variables - Runtime overrides (highest priority)

**To override settings locally:**

Create `appsettings.Development.local.json` in `src/PropertyManagement.Web/`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=MyLocalDB;Username=myuser;Password=mysecretpassword"
  }
}
```

This file is ignored by git, keeping your secrets safe.

### Database Migrations

The application uses EF Core migrations. In development, the database is created automatically on startup.

**To create a new migration:**

```bash
cd src/PropertyManagement.Web
dotnet ef migrations add <MigrationName>
```

**To apply migrations manually:**

```bash
dotnet ef database update
```

## Key Features Implemented

### Submit Maintenance Request (First Vertical Slice)

**Path:** `/maintenance-requests/submit`

**Features:**
- Property and unit selection
- Urgency level (Low, Medium, High, Emergency)
- Rich text description
- Form validation using FluentValidation
- Optimistic UI updates with Fluxor
- Domain-driven business rules

**Business Rules:**
- Emergency work orders bypass normal scheduling
- Work orders can only be created for valid properties
- Description must be at least 10 characters
- Status transitions follow defined workflow

**Try it out:**
1. Navigate to "Submit Maintenance Request"
2. Select a property
3. Choose urgency level
4. Provide detailed description
5. Submit and observe optimistic update

## Domain Rules Enforced

1. **WorkOrder Lifecycle:**
   - Can only transition to valid next states
   - Cannot be completed without vendor assignment
   - Emergency priority requires immediate attention

2. **Vendor Assignment:**
   - Vendors can only handle work matching their specialization
   - Technicians must be available for assignment

3. **Property Management:**
   - Units must belong to a property
   - Unit numbers must be unique within a property

4. **Scheduling:**
   - Preventive maintenance auto-generates work orders
   - No overlapping vendor assignments

## Learning Objectives

### Domain-Driven Design

- **Aggregates** with clear boundaries (`WorkOrder`, `Property`, `Vendor`)
- **Value Objects** for concepts without identity (`Money`, `Address`, `DateRange`)
- **Domain Events** for cross-aggregate communication
- **Domain Services** for operations spanning multiple aggregates
- **Ubiquitous Language** in code and documentation

### CQRS with MediatR

- Commands modify state (`SubmitMaintenanceRequestCommand`)
- Handlers encapsulate business logic
- Pipeline behaviors for cross-cutting concerns (logging)
- Separation of read and write models

### Vertical Slice Architecture

- Features are self-contained (command, handler, validator, UI in one place)
- Reduces coupling between features
- Easy to add new features without affecting existing code
- Clear feature boundaries

### Event-Driven Design

- Domain events raised by aggregates
- Events represent facts that occurred
- Enable reactive behavior and notifications
- TODO: Publish events for integration

## TODO: Future Enhancements

The codebase includes TODO comments for learning extensions:

- [ ] Domain event publishing and handlers
- [ ] Integration events for external systems
- [ ] Complete optimistic update implementation
- [ ] SignalR for real-time notifications
- [ ] Additional validation rules
- [ ] WorkOrderSchedulingService integration
- [ ] Vendor rating and selection algorithm
- [ ] Reports and analytics
- [ ] Authentication and authorization
- [ ] Multi-tenancy support

## Development Guidelines

### Adding a New Vertical Slice

1. Create feature folder under `Features/`
2. Create command/query with handler
3. Add FluentValidation validator
4. Create Razor component for UI
5. Add navigation link in `NavMenu.razor`
6. Update Fluxor state if needed

### Domain Model Changes

1. Update aggregate root
2. Add/modify value objects
3. Create entity configurations
4. Generate EF Core migration
5. Update handlers using the domain model

## Testing the Application

### Manual Testing Steps

1. **Submit a maintenance request**
   - Use the form to create a work order
   - Verify validation works
   - Check database persistence

2. **Observe domain rules**
   - Try invalid state transitions
   - Test emergency priority handling
   - Verify business logic in aggregates

3. **State management**
   - Submit a request and observe optimistic update
   - Check error handling on failure
   - Verify Redux DevTools (if enabled)

## Architecture Decisions

### Why Vertical Slice Architecture?

- **Cohesion**: Related code lives together
- **Independence**: Features don't interfere with each other
- **Scalability**: Easy to add features without refactoring
- **Team productivity**: Different developers can work on different slices

### Why Rich Domain Models?

- **Encapsulation**: Business logic lives in the domain
- **Validation**: Invariants enforced by the model
- **Expressiveness**: Code reflects business language
- **Testability**: Pure domain logic is easy to test

### Why Fluxor for State Management?

- **Predictability**: Unidirectional data flow
- **Debugging**: Time-travel debugging with Redux DevTools
- **Optimistic updates**: Better UX with immediate feedback
- **Reactive UI**: Automatic re-rendering on state changes

## Resources

- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)
- [Vertical Slice Architecture by Jimmy Bogard](https://www.jimmybogard.com/vertical-slice-architecture/)
- [CQRS by Martin Fowler](https://martinfowler.com/bliki/CQRS.html)
- [Fluxor Documentation](https://github.com/mrpmorris/Fluxor)
- [MediatR Documentation](https://github.com/jbogard/MediatR)

## License

This is a learning project. Feel free to use and modify as needed.

## Contributing

This is a learning project, but suggestions and improvements are welcome!
