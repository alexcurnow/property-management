namespace PropertyManagement.Web.Features.MaintenanceRequests.Submit.Models;

/// <summary>
/// WorkOrder model optimized for the Submit Maintenance Request feature
/// This is NOT a shared domain model - it belongs to THIS slice only
/// </summary>
public class WorkOrder
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public Guid? UnitId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "New";
    public int PriorityLevel { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public decimal EstimatedCostAmount { get; set; }
    public string EstimatedCostCurrency { get; set; } = "USD";
    public DateTime CreatedAt { get; set; }

    // Navigation properties (minimal - only what this slice needs)
    public Property? Property { get; set; }
    public Unit? Unit { get; set; }
}

/// <summary>
/// Minimal Property model for this slice - just what we need for validation
/// </summary>
public class Property
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Minimal Unit model for this slice
/// </summary>
public class Unit
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
}

/// <summary>
/// Priority value object - specific to this slice
/// Simpler than the shared version - just what we need for submission
/// </summary>
public static class WorkOrderPriority
{
    public static (int Level, string Name) FromUrgency(string urgencyLevel)
    {
        return urgencyLevel switch
        {
            "Low" => (1, "Low"),
            "Medium" => (2, "Medium"),
            "High" => (3, "High"),
            "Emergency" => (4, "Emergency"),
            _ => throw new ArgumentException($"Invalid urgency level: {urgencyLevel}")
        };
    }
}
