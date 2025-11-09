namespace PropertyManagement.Web.Domain.WorkOrders.Events;

/// <summary>
/// Domain event raised when a work order is created
/// </summary>
public record WorkOrderCreatedEvent
{
    public Guid WorkOrderId { get; init; }
    public Guid PropertyId { get; init; }
    public Guid? UnitId { get; init; }
    public string Description { get; init; }
    public WorkOrderPriority Priority { get; init; }
    public DateTime CreatedAt { get; init; }

    public WorkOrderCreatedEvent(
        Guid workOrderId,
        Guid propertyId,
        Guid? unitId,
        string description,
        WorkOrderPriority priority,
        DateTime createdAt)
    {
        WorkOrderId = workOrderId;
        PropertyId = propertyId;
        UnitId = unitId;
        Description = description;
        Priority = priority;
        CreatedAt = createdAt;
    }
}
