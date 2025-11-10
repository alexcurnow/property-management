# Property Management System - Maintenance Operations

A learning project demonstrating **Vertical Slice Architecture (VSA)**, **CQRS**, and **Domain-Driven Design principles** using .NET 9 and Blazor Interactive Server-Side Rendering.

## Architecture Overview

### Design Patterns & Principles

- **Vertical Slice Architecture**: Complete features organized by business capability, self-contained with everything they need
- **CQRS**: Command-Query separation using MediatR (commands modify, queries read)
- **DDD Principles**: Value objects, ubiquitous language, business rules in handlers
- **Duplication over Wrong Abstraction**: Each slice owns its models to prevent coupling
- **Feature Independence**: Features communicate only through the database

### Technology Stack

- **.NET 9** with C# 12
- **Blazor Interactive SSR** (Server-Side Rendering)
- **MudBlazor** for UI components
- **Fluxor** for state management and optimistic updates
- **MediatR** for CQRS implementation
- **FluentValidation** for input validation
- **Entity Framework Core 9** with PostgreSQL
- **Docker** for containerization

## What is Vertical Slice Architecture?

VSA organizes code by **feature** rather than by technical layer. Each feature (slice) contains:

- **Models** - Data structures optimized for the feature's use case
- **Handlers** - Business logic for commands and queries
- **Validators** - Input validation rules
- **UI Components** - Razor pages/components
- **Database Configs** - EF Core entity configurations

### Key Principles

1. **Feature Independence**: Each slice is self-contained
2. **Duplication Over Abstraction**: OK to duplicate models between slices to avoid coupling
3. **Single Touchpoint**: Slices only interact through the shared database
4. **Optimize Per Use Case**: Each slice's models are optimized for its specific needs

Example: The Submit slice has a `WorkOrder` model with fields needed for submission, while the Complete slice has a `WorkOrder` model with completion-specific fields. They map to the same database table but serve different purposes.

## Project Structure

```
PropertyManagement/
├── docker-compose.yml
├── Dockerfile
└── src/
    └── PropertyManagement.Web/
        ├── Features/                           # Vertical Slices
        │   ├── MaintenanceRequests/
        │   │   ├── Submit/                    # Submit new maintenance requests
        │   │   │   ├── Models/                # Slice-specific models (WorkOrder, Property, Unit)
        │   │   │   │   └── WorkOrder.cs
        │   │   │   ├── Database/              # EF Core configurations for this slice
        │   │   │   │   └── WorkOrderConfiguration.cs
        │   │   │   ├── SubmitMaintenanceRequest.cs      # Command, Handler, Validator
        │   │   │   └── SubmitMaintenanceRequest.razor   # UI Component
        │   │   ├── Review/                    # Review pending requests
        │   │   │   ├── ReviewMaintenanceRequest.cs      # Query, Handler
        │   │   │   └── ReviewMaintenanceRequest.razor
        │   │   ├── Assign/                    # Assign work orders to vendors
        │   │   │   ├── Models/
        │   │   │   ├── Database/
        │   │   │   ├── AssignMaintenanceRequest.cs
        │   │   │   └── AssignMaintenanceRequest.razor
        │   │   └── Complete/                  # Mark work orders as complete
        │   │       ├── Models/
        │   │       ├── Database/
        │   │       ├── CompleteWorkOrder.cs
        │   │       └── CompleteWorkOrder.razor
        │   └── Properties/
        │       └── List/                      # List all properties (query slice)
        │           └── ListProperties.cs
        ├── Common/                             # Truly universal code
        │   ├── ValueObjects/
        │   │   └── Money.cs                   # Shared value object
        │   └── Behaviors/
        │       └── LoggingBehavior.cs         # MediatR pipeline behavior
        ├── Infrastructure/                     # Technical infrastructure
        │   └── Persistence/
        │       ├── PropertyManagementDbContext.cs  # Aggregates configurations from all slices
        │       └── Migrations/
        └── Shared/                             # Cross-cutting UI
            ├── Components/Layout/
            └── State/                         # Fluxor state management
```

### Architecture Highlights

- **No Domain Layer**: Domain logic lives in handlers within each slice
- **No Shared Models** (except Common): Each slice defines its own models
- **Database as Integration Point**: The only place slices interact
- **DbContext Aggregates Configs**: Each slice registers its entity configurations

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

## Implemented Features (Vertical Slices)

### 1. Submit Maintenance Request

**Path:** `/maintenance-requests/submit`

**Type:** Command Slice (Write Operation)

**Features:**
- Select property and optional unit
- Set urgency level (Low, Medium, High, Emergency)
- Provide detailed description (validated, min 10 chars)
- Optimistic UI updates with Fluxor state management
- Form validation using FluentValidation

**Business Rules:**
- Emergency work orders are logged for immediate attention
- Must select a valid property
- Description must be detailed (10-1000 characters)
- Priority level determines urgency

**Models:** `WorkOrder`, `Property`, `Unit` (optimized for submission)

### 2. Review Maintenance Requests

**Path:** `/maintenance-requests/review`

**Type:** Query Slice (Read Operation)

**Features:**
- View all pending ("New" status) work orders
- Sorted by priority (highest first), then creation date
- Display property name, description, status, and created date
- Priority badges with color coding
- Limited to 50 most recent requests

**Models:** `WorkOrderReviewDto` (read-optimized)

### 3. Assign Work Orders to Vendors

**Path:** `/maintenance-requests/assign`

**Type:** Command Slice (Write Operation)

**Features:**
- View unassigned work orders
- Select a work order to assign
- Choose from active vendors
- Optionally set estimated cost
- Validation prevents double-assignment

**Business Rules:**
- Can only assign unassigned work orders
- Vendor must be active
- Status changes from "New" to "Assigned"
- Tracks assignment timestamp

**Models:** `WorkOrder`, `Vendor` (with assignment fields)

### 4. Complete Work Orders

**Path:** `/maintenance-requests/complete`

**Type:** Command Slice (Write Operation)

**Features:**
- View in-progress (assigned) work orders
- Add completion notes (required, min 10 chars)
- Enter actual cost
- Warning if cost exceeds estimate by >20%
- Tracks completion timestamp

**Business Rules:**
- Can only complete assigned work orders
- Cannot complete already completed work orders
- Completion notes are mandatory
- Logs warnings for significant cost overruns

**Models:** `WorkOrder` (with completion fields), `Vendor`, `Property`

### 5. List Properties

**Path:** Used by other slices

**Type:** Query Slice (Read Operation)

**Features:**
- Simple property listing
- Includes list of units per property
- Used by submission and other forms

## Business Rules Enforced

1. **Work Order Lifecycle:**
   - New → Assigned → Completed (one-way flow)
   - Cannot skip assignment step
   - Emergency priority logs warnings

2. **Vendor Assignment:**
   - Only active vendors can be assigned
   - Prevents double-assignment
   - Tracks who assigned and when

3. **Cost Management:**
   - Estimated cost set during assignment
   - Actual cost captured at completion
   - Warns if actual exceeds estimate by 20%+

4. **Validation:**
   - Property must exist
   - Unit must belong to property (if specified)
   - Descriptions must be detailed (10+ chars)
   - Costs must be non-negative

## Learning Objectives

### Vertical Slice Architecture

- **Feature Cohesion**: All code for a feature lives together
- **Model Optimization**: Each slice has models optimized for its use case
- **Independence**: Slices don't depend on each other
- **Duplication is OK**: Better to duplicate models than create wrong abstractions
- **Easy to Add Features**: New slices don't affect existing ones

### CQRS with MediatR

- **Commands**: Modify state (`SubmitMaintenanceRequestCommand`, `AssignMaintenanceRequestCommand`)
- **Queries**: Read data (`ReviewMaintenanceRequestsQuery`, `ListPropertiesQuery`)
- **Handlers**: Encapsulate business logic for each command/query
- **Pipeline Behaviors**: Cross-cutting concerns (logging) via `LoggingBehavior`
- **Validation**: FluentValidation runs before handlers

### DDD Principles Applied

- **Value Objects**: `Money` in Common (truly universal)
- **Ubiquitous Language**: Code reflects business terminology (WorkOrder, Vendor, Priority)
- **Business Rules in Handlers**: Logic lives close to the feature
- **Encapsulation**: Models protect their invariants

### Fluxor State Management

- **Optimistic Updates**: UI updates immediately before server response
- **Unidirectional Data Flow**: State → UI → Actions → State
- **Predictability**: State changes are explicit via actions/reducers
- **Effects**: Handle side effects (API calls) separately from reducers

## Development Guidelines

### Adding a New Vertical Slice

1. **Create feature folder** under `Features/FeatureArea/FeatureName/`

2. **Define slice-specific models** in `Models/`
   ```csharp
   public class WorkOrder  // Optimized for THIS slice's needs
   {
       public Guid Id { get; set; }
       // Only fields needed for this feature
   }
   ```

3. **Create EF configurations** in `Database/`
   ```csharp
   public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
   {
       public void Configure(EntityTypeBuilder<WorkOrder> builder)
       {
           builder.ToTable("WorkOrders");  // Same table, different view
           // Configure fields needed by this slice
       }
   }
   ```

4. **Create command/query with handler**
   ```csharp
   public record MyCommand : IRequest<MyResponse>;
   public class MyHandler : IRequestHandler<MyCommand, MyResponse>
   {
       // Business logic here
   }
   ```

5. **Add FluentValidation validator**
   ```csharp
   public class MyValidator : AbstractValidator<MyCommand>
   {
       public MyValidator()
       {
           RuleFor(x => x.Field).NotEmpty();
       }
   }
   ```

6. **Create Razor component** for UI
   ```razor
   @page "/my-feature"
   @using MediatR
   @inject IMediator Mediator
   ```

7. **Register entity configurations** in `PropertyManagementDbContext.cs`
   ```csharp
   modelBuilder.ApplyConfiguration(
       new Features.MyFeature.Database.WorkOrderConfiguration());
   ```

8. **Add navigation** in `NavMenu.razor`

### Key Principles to Follow

- **Duplication Over Abstraction**: Don't share models between slices "just because"
- **Optimize for Use Case**: Each slice's models should fit its specific needs
- **Keep Slices Independent**: Avoid referencing one slice from another
- **Business Logic in Handlers**: Don't create separate domain services
- **Truly Common Code Only**: Only put code in `Common/` if it's used by multiple slices

## Architecture Decisions

### Why Vertical Slice Architecture?

- **Cohesion**: Related code lives together (models, logic, UI)
- **Independence**: Features don't interfere with each other
- **Velocity**: Easy to add features without refactoring existing code
- **Team Scalability**: Different devs can work on different slices
- **Cognitive Load**: Understand one feature at a time

### Why Allow Model Duplication?

- **Coupling is Worse**: Shared models create coupling between features
- **Optimize Per Use Case**: Submit needs different fields than Complete
- **Change Independence**: Can change one slice without affecting others
- **Database is the Contract**: Slices agree on table structure, not models

### Why Fluxor for State Management?

- **Predictability**: Unidirectional data flow makes debugging easier
- **Optimistic Updates**: Better UX with immediate feedback
- **Time-Travel Debugging**: Redux DevTools (when enabled)
- **Reactive UI**: Automatic re-rendering on state changes

## Future Enhancements

- [ ] Schedule Preventive Maintenance slice
- [ ] Vendor Management slice (add/edit vendors)
- [ ] Property Management slice (add/edit properties)
- [ ] Work Order History slice (completed work orders)
- [ ] Cost Reporting slice (analytics)
- [ ] SignalR for real-time notifications
- [ ] Authentication and authorization
- [ ] Multi-tenancy support

## Resources

- [Vertical Slice Architecture by Jimmy Bogard](https://www.jimmybogard.com/vertical-slice-architecture/)
- [CQRS by Martin Fowler](https://martinfowler.com/bliki/CQRS.html)
- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)
- [Fluxor Documentation](https://github.com/mrpmorris/Fluxor)
- [MediatR Documentation](https://github.com/jbogard/MediatR)

## License

This is a learning project. Feel free to use and modify as needed.

## Contributing

This is a learning project, but suggestions and improvements are welcome!
