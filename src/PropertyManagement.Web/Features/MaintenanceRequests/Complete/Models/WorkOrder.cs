namespace PropertyManagement.Web.Features.MaintenanceRequests.Complete.Models;

// ============================================================================
// WORK ORDER - Complete slice model (focused on completion fields)
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

    // Assignment fields
    public Guid? AssignedVendorId { get; set; }
    public DateTime? AssignedAt { get; set; }

    // Estimated cost fields
    public decimal EstimatedCostAmount { get; set; }
    public string EstimatedCostCurrency { get; set; } = "USD";

    // Completion-specific fields
    public DateTime? CompletedAt { get; set; }
    public Guid? CompletedBy { get; set; } // User or Vendor who marked it complete
    public string CompletionNotes { get; set; } = string.Empty;
    public decimal ActualCostAmount { get; set; }
    public string ActualCostCurrency { get; set; } = "USD";

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties for EF Core
    public Vendor? Vendor { get; set; }
    public Property? Property { get; set; }
}

// ============================================================================
// VENDOR - Minimal model for this slice
// ============================================================================

public class Vendor
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
}

// ============================================================================
// PROPERTY - Minimal model for this slice
// ============================================================================

public class Property
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
