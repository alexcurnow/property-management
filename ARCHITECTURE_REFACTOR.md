# Architecture Refactor: Clean Architecture → Vertical Slice Architecture

## Current Architecture (Clean Architecture)

```
src/PropertyManagement.Web/
├── Domain/                      # ❌ Shared domain layer
│   ├── WorkOrders/
│   │   ├── WorkOrder.cs
│   │   ├── WorkOrderStatus.cs
│   │   └── WorkOrderPriority.cs
│   ├── Properties/
│   ├── Vendors/
│   └── Common/
│       └── Money.cs
├── Infrastructure/              # ❌ Shared infrastructure
│   ├── Persistence/
│   │   ├── PropertyManagementDbContext.cs
│   │   └── Configurations/
│   └── Behaviors/
└── Features/                    # ⚠️ Only UI + handlers
    └── MaintenanceRequests/
        └── Submit/
            ├── SubmitMaintenanceRequest.cs      # Handler + Command
            └── SubmitMaintenanceRequest.razor   # UI
```

**Problems with Clean Architecture for learning:**
- Shared domain models encourage coupling
- Changes ripple across layers
- Hard to see complete feature in one place
- Domain models try to serve all use cases
- Not optimized for change

---

## Target Architecture (Vertical Slice)

```
src/PropertyManagement.Web/
├── Features/
│   └── MaintenanceRequests/
│       └── Submit/
│           ├── SubmitMaintenanceRequest.cs       # ✅ Command + Handler + Validator
│           ├── SubmitMaintenanceRequest.razor    # ✅ UI Component
│           ├── WorkOrderModels.cs                # ✅ Feature-specific models
│           ├── SubmitRequestValidator.cs         # ✅ Validation
│           └── Database.cs                       # ✅ EF Config for THIS feature
│
├── Common/                      # ✅ Minimal shared code
│   ├── Database/
│   │   └── AppDbContext.cs     # Aggregates all feature configs
│   ├── Behaviors/
│   │   └── LoggingBehavior.cs
│   └── ValueObjects/           # Only truly shared value objects
│       └── Money.cs
│
└── Shared/                      # ✅ UI components only
    └── Components/
```

**Benefits of Vertical Slice:**
- ✅ Complete feature in one folder
- ✅ Easy to add/remove features
- ✅ Models optimized for specific use case
- ✅ No shared domain to maintain
- ✅ Changes isolated to one slice
- ✅ Clear feature boundaries
- ✅ Better for learning - see everything in context

---

## Key Principles of Vertical Slice Architecture

### 1. **Slice = Complete Feature**
Each slice contains EVERYTHING needed for that feature:
- Request/Response models
- Command/Query handlers
- Validation logic
- Database configuration
- UI components
- Business rules specific to that feature

### 2. **Duplication > Wrong Abstraction**
- OK to duplicate models between slices
- Each slice optimized for its use case
- Don't force shared domain models

### 3. **Minimal Shared Code**
Only share what's truly cross-cutting:
- Database context (aggregates feature configs)
- MediatR pipeline behaviors
- Authentication/Authorization
- Universal value objects (Money, Email)

### 4. **Database Per Feature**
Each slice owns its database configuration:
```csharp
// In Features/MaintenanceRequests/Submit/Database.cs
public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    // This WorkOrder model belongs to THIS slice only
}
```

### 5. **CQRS Naturally Emerges**
- Commands and Queries are separate slices
- Each optimized for its purpose
- Read models != Write models

---

## Refactoring Strategy

### Phase 1: Restructure First Slice
**Feature**: Submit Maintenance Request

**Move:**
1. Create `Features/MaintenanceRequests/Submit/Models/`
   - Copy `WorkOrder.cs` → rename to `SubmitWorkOrder.cs`
   - Copy `WorkOrderPriority.cs`
   - Copy `WorkOrderStatus.cs`
   - Keep only what THIS slice needs

2. Create `Features/MaintenanceRequests/Submit/Database/`
   - Move entity configuration
   - Create migration specific to this feature

3. Keep in `Features/MaintenanceRequests/Submit/`:
   - `SubmitMaintenanceRequest.cs` (Command + Handler)
   - `SubmitMaintenanceRequestValidator.cs`
   - `SubmitMaintenanceRequest.razor`

### Phase 2: Move Common Code
Create `Common/` directory for:
- `Money.cs` - truly universal value object
- `AppDbContext.cs` - aggregates feature configurations
- MediatR behaviors

### Phase 3: Refactor Remaining Slices
- Review Maintenance Request
- Assign Maintenance Request
- Complete Work Order
- Schedule Preventive Maintenance

### Phase 4: Delete Old Structure
Remove:
- `Domain/` directory
- `Infrastructure/` directory (keep behaviors in Common)

---

## Example: Submit Maintenance Request Slice

### Complete Slice Structure

```
Features/MaintenanceRequests/Submit/
├── SubmitMaintenanceRequest.cs
│   ├── SubmitCommand
│   ├── SubmitCommandValidator
│   ├── SubmitCommandHandler
│   └── SubmitResponse
│
├── SubmitMaintenanceRequest.razor
│   └── UI Component
│
├── Models/
│   ├── WorkOrder.cs              # This slice's WorkOrder
│   ├── WorkOrderPriority.cs
│   └── WorkOrderStatus.cs
│
└── Database/
    └── WorkOrderConfiguration.cs
```

### Code Example

```csharp
// Features/MaintenanceRequests/Submit/SubmitMaintenanceRequest.cs
namespace PropertyManagement.Features.MaintenanceRequests.Submit;

// ============ COMMAND ============
public record SubmitCommand : IRequest<SubmitResponse>
{
    public Guid PropertyId { get; init; }
    public string Description { get; init; } = string.Empty;
    public string UrgencyLevel { get; init; } = string.Empty;
}

// ============ VALIDATOR ============
public class SubmitCommandValidator : AbstractValidator<SubmitCommand>
{
    public SubmitCommandValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty();
        RuleFor(x => x.Description).MinimumLength(10).MaximumLength(1000);
        RuleFor(x => x.UrgencyLevel).Must(BeValid);
    }

    private bool BeValid(string level) =>
        new[] { "Low", "Medium", "High", "Emergency" }.Contains(level);
}

// ============ HANDLER ============
public class SubmitCommandHandler : IRequestHandler<SubmitCommand, SubmitResponse>
{
    private readonly AppDbContext _db;

    public async Task<SubmitResponse> Handle(
        SubmitCommand request,
        CancellationToken ct)
    {
        // Create WorkOrder using THIS slice's model
        var workOrder = new WorkOrder
        {
            Id = Guid.NewGuid(),
            PropertyId = request.PropertyId,
            Description = request.Description,
            Priority = WorkOrderPriority.FromString(request.UrgencyLevel),
            Status = WorkOrderStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        _db.WorkOrders.Add(workOrder);
        await _db.SaveChangesAsync(ct);

        return new SubmitResponse(true, workOrder.Id);
    }
}

// ============ RESPONSE ============
public record SubmitResponse(bool Success, Guid WorkOrderId);
```

---

## Migration Plan

### Step 1: Create New Structure (Keep Old)
- Build new vertical slices alongside old code
- Don't delete anything yet
- Verify new slices work

### Step 2: Update References
- Update Program.cs registrations
- Update UI navigation
- Update tests

### Step 3: Remove Old Structure
- Delete Domain/ directory
- Delete Infrastructure/ directory
- Clean up unused code

### Step 4: Document
- Update README
- Add architecture decision records
- Document patterns

---

## Trade-offs

### Vertical Slice Advantages ✅
- Easy to understand complete feature
- Easy to add/remove features
- Optimized for change
- Clear boundaries
- Better for learning
- Reduced coupling

### Vertical Slice Challenges ⚠️
- Some duplication across slices
- Shared concerns need discipline
- Database migrations more complex
- Need consistent patterns

### When to Use Each

**Vertical Slice:**
- ✅ CRUD applications
- ✅ Feature-rich applications
- ✅ High rate of change
- ✅ Large teams
- ✅ Microservices
- ✅ Learning DDD/CQRS

**Clean Architecture:**
- ✅ Complex domain logic
- ✅ Stable requirements
- ✅ Reusable domain models
- ✅ Multiple UI frontends
- ✅ Long-term maintenance

---

## Next Steps

1. ✅ Review this document
2. ⏭️ Refactor first slice (Submit Maintenance Request)
3. ⏭️ Extract common code
4. ⏭️ Refactor remaining slices
5. ⏭️ Clean up old structure
6. ⏭️ Update documentation
7. ⏭️ Test everything

---

## Questions to Consider

1. **How much should we share?**
   - Only truly universal concepts (Money, Email, Address?)
   - Each slice can have its own models

2. **How to handle database?**
   - Each slice registers its entity configurations
   - AppDbContext aggregates them all

3. **What about domain events?**
   - Keep them! Each slice can publish events
   - Events are integration points between slices

4. **How to handle read vs write models?**
   - Separate slices for queries vs commands
   - Read models optimized for display
   - Write models optimized for validation/business rules
