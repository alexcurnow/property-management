using PropertyManagement.Web.Domain.Common;
using PropertyManagement.Web.Domain.WorkOrders.Events;

namespace PropertyManagement.Web.Domain.WorkOrders;

/// <summary>
/// Aggregate root representing a maintenance work order with rich business rules
/// </summary>
public class WorkOrder
{
    public Guid Id { get; private set; }
    public Guid PropertyId { get; private set; }
    public Guid? UnitId { get; private set; }
    public string Description { get; private set; }
    public WorkOrderStatus Status { get; private set; }
    public WorkOrderPriority Priority { get; private set; }
    public Guid? AssignedVendorId { get; private set; }
    public Guid? AssignedTechnicianId { get; private set; }
    public Money EstimatedCost { get; private set; }
    public Money? ActualCost { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? AssignedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? ScheduledFor { get; private set; }
    public string? CompletionNotes { get; private set; }

    // Domain events collection
    private readonly List<object> _domainEvents = new();
    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    // EF Core constructor - initializes non-nullable properties to satisfy compiler
    private WorkOrder()
    {
        Description = null!;
        Status = null!;
        Priority = null!;
        EstimatedCost = null!;
    }

    private WorkOrder(Guid propertyId, Guid? unitId, string description, WorkOrderPriority priority)
    {
        if (propertyId == Guid.Empty)
            throw new ArgumentException("Property ID cannot be empty", nameof(propertyId));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        Id = Guid.NewGuid();
        PropertyId = propertyId;
        UnitId = unitId;
        Description = description;
        Status = WorkOrderStatus.New;
        Priority = priority ?? throw new ArgumentNullException(nameof(priority));
        EstimatedCost = Money.Zero();
        CreatedAt = DateTime.UtcNow;

        // Raise domain event
        // TODO: Add domain event publishing here
        _domainEvents.Add(new WorkOrderCreatedEvent(
            Id, PropertyId, UnitId, Description, Priority, CreatedAt));
    }

    public static WorkOrder Create(Guid propertyId, Guid? unitId, string description, WorkOrderPriority priority)
    {
        return new WorkOrder(propertyId, unitId, description, priority);
    }

    public void AssignToVendor(Guid vendorId, Guid? technicianId, DateTime? scheduledFor = null)
    {
        if (!Status.CanTransitionTo(WorkOrderStatus.Assigned))
            throw new InvalidOperationException($"Cannot assign work order in {Status} status");

        if (vendorId == Guid.Empty)
            throw new ArgumentException("Vendor ID cannot be empty", nameof(vendorId));

        AssignedVendorId = vendorId;
        AssignedTechnicianId = technicianId;
        AssignedAt = DateTime.UtcNow;
        ScheduledFor = scheduledFor;
        Status = WorkOrderStatus.Assigned;

        // TODO: Add domain event publishing here
        _domainEvents.Add(new WorkOrderAssignedEvent(
            Id, vendorId, technicianId, AssignedAt.Value));
    }

    public void StartWork()
    {
        if (!Status.CanTransitionTo(WorkOrderStatus.InProgress))
            throw new InvalidOperationException($"Cannot start work order in {Status} status");

        if (AssignedVendorId == null)
            throw new InvalidOperationException("Cannot start work order without vendor assignment");

        Status = WorkOrderStatus.InProgress;
    }

    public void Complete(Money actualCost, string completionNotes)
    {
        if (!Status.CanTransitionTo(WorkOrderStatus.Completed))
            throw new InvalidOperationException($"Cannot complete work order in {Status} status");

        if (AssignedVendorId == null)
            throw new InvalidOperationException("Cannot complete work order without vendor assignment");

        if (actualCost == null)
            throw new ArgumentNullException(nameof(actualCost));

        ActualCost = actualCost;
        CompletionNotes = completionNotes;
        CompletedAt = DateTime.UtcNow;
        Status = WorkOrderStatus.Completed;
    }

    public void Cancel()
    {
        if (!Status.CanTransitionTo(WorkOrderStatus.Cancelled))
            throw new InvalidOperationException($"Cannot cancel work order in {Status} status");

        Status = WorkOrderStatus.Cancelled;
    }

    public void UpdateEstimatedCost(Money estimatedCost)
    {
        EstimatedCost = estimatedCost ?? throw new ArgumentNullException(nameof(estimatedCost));
    }

    public void UpdatePriority(WorkOrderPriority newPriority)
    {
        if (Status == WorkOrderStatus.Completed || Status == WorkOrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot update priority of completed or cancelled work order");

        Priority = newPriority ?? throw new ArgumentNullException(nameof(newPriority));
    }

    public bool RequiresImmediateAttention()
    {
        return Priority.IsEmergency();
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
