using PropertyManagement.Web.Domain.Scheduling;
using PropertyManagement.Web.Domain.Vendors;
using PropertyManagement.Web.Domain.WorkOrders;

namespace PropertyManagement.Web.Domain.Services;

/// <summary>
/// Domain service that coordinates work order scheduling across aggregates
/// Handles complex business logic involving vendor availability and property access windows
/// </summary>
public class WorkOrderSchedulingService
{
    /// <summary>
    /// Determines if a work order can be scheduled for a vendor during a specific time window
    /// </summary>
    public bool CanScheduleWorkOrder(
        WorkOrder workOrder,
        Vendor vendor,
        DateRange requestedWindow,
        IEnumerable<WorkOrder> vendorExistingWorkOrders)
    {
        if (workOrder == null)
            throw new ArgumentNullException(nameof(workOrder));

        if (vendor == null)
            throw new ArgumentNullException(nameof(vendor));

        if (requestedWindow == null)
            throw new ArgumentNullException(nameof(requestedWindow));

        // Emergency work orders bypass normal scheduling
        if (workOrder.RequiresImmediateAttention())
            return vendor.IsActive;

        // Check vendor is active
        if (!vendor.IsActive)
            return false;

        // Check for scheduling conflicts
        var hasConflict = vendorExistingWorkOrders
            .Where(wo => wo.ScheduledFor.HasValue)
            .Any(wo =>
            {
                var existingWindow = DateRange.Create(
                    wo.ScheduledFor!.Value,
                    wo.ScheduledFor.Value.AddHours(4)); // Assume 4-hour default window
                return existingWindow.Overlaps(requestedWindow);
            });

        return !hasConflict;
    }

    /// <summary>
    /// Finds the best vendor for a work order based on specialization and availability
    /// </summary>
    public Vendor? FindBestVendor(
        WorkOrder workOrder,
        VendorSpecialization requiredSpecialization,
        IEnumerable<Vendor> availableVendors,
        DateRange? preferredWindow = null)
    {
        if (workOrder == null)
            throw new ArgumentNullException(nameof(workOrder));

        // Filter vendors by specialization and active status
        var qualifiedVendors = availableVendors
            .Where(v => v.CanHandleWorkOrder(requiredSpecialization))
            .ToList();

        if (!qualifiedVendors.Any())
            return null;

        // For emergency work orders, return first available vendor
        if (workOrder.RequiresImmediateAttention())
            return qualifiedVendors.FirstOrDefault();

        // TODO: Integrate with WorkOrderSchedulingService
        // Future: Add logic for vendor rating, response time, cost, etc.
        return qualifiedVendors.FirstOrDefault();
    }

    /// <summary>
    /// Calculates the optimal scheduling window based on work order priority
    /// </summary>
    public DateRange CalculateOptimalWindow(WorkOrder workOrder)
    {
        if (workOrder == null)
            throw new ArgumentNullException(nameof(workOrder));

        var now = DateTime.UtcNow;

        return workOrder.Priority.Level switch
        {
            4 => DateRange.Create(now, now.AddHours(4)),      // Emergency: 4 hours
            3 => DateRange.Create(now, now.AddHours(24)),     // High: 24 hours
            2 => DateRange.Create(now, now.AddDays(3)),       // Medium: 3 days
            1 => DateRange.Create(now, now.AddDays(7)),       // Low: 7 days
            _ => DateRange.Create(now, now.AddDays(7))
        };
    }
}
