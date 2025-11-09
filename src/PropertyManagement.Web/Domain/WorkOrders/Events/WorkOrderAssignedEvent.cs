namespace PropertyManagement.Web.Domain.WorkOrders.Events;

/// <summary>
/// Domain event raised when a work order is assigned to a vendor/technician
/// </summary>
public record WorkOrderAssignedEvent
{
    public Guid WorkOrderId { get; init; }
    public Guid VendorId { get; init; }
    public Guid? TechnicianId { get; init; }
    public DateTime AssignedAt { get; init; }

    public WorkOrderAssignedEvent(
        Guid workOrderId,
        Guid vendorId,
        Guid? technicianId,
        DateTime assignedAt)
    {
        WorkOrderId = workOrderId;
        VendorId = vendorId;
        TechnicianId = technicianId;
        AssignedAt = assignedAt;
    }
}
