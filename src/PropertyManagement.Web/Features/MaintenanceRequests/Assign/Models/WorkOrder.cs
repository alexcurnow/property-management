namespace PropertyManagement.Web.Features.MaintenanceRequests.Assign.Models;

// ============================================================================
// WORK ORDER - Assign slice model (focused on assignment fields)
// ============================================================================

public class WorkOrder
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public Guid? UnitId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "New";
    public int PriorityLevel { get; set; }
    public string PriorityName { get; set; } = string.Empty;

    // Assignment-specific fields
    public Guid? AssignedVendorId { get; set; }
    public DateTime? AssignedAt { get; set; }
    public Guid? AssignedBy { get; set; } // User who assigned

    // Estimated cost fields
    public decimal EstimatedCostAmount { get; set; }
    public string EstimatedCostCurrency { get; set; } = "USD";

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties for EF Core
    public Vendor? Vendor { get; set; }
    public Property? Property { get; set; }
}

// ============================================================================
// VENDOR - Assign slice model
// ============================================================================

public class Vendor
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

// ============================================================================
// PROPERTY - Minimal model for this slice
// ============================================================================

public class Property
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
