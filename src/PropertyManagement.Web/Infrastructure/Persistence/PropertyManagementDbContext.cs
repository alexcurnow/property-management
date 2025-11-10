using Microsoft.EntityFrameworkCore;

namespace PropertyManagement.Web.Infrastructure.Persistence;

/// <summary>
/// Application DbContext that aggregates configurations from all vertical slices
/// Each feature registers its own entity configurations
/// </summary>
public class PropertyManagementDbContext : DbContext
{
    public PropertyManagementDbContext(DbContextOptions<PropertyManagementDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from all vertical slices
        // Each slice owns its own entity configurations
        ApplyConfigurationsFromSlices(modelBuilder);
    }

    private void ApplyConfigurationsFromSlices(ModelBuilder modelBuilder)
    {
        // Apply entity configurations from slices
        // NOTE: In true VSA, each slice has its own models, but they map to shared tables.
        // We only apply the configuration ONCE per table to avoid EF Core conflicts.
        // The Submit slice has the most complete configuration, so we use that as the source.

        // Submit Maintenance Request slice - provides base configurations
        modelBuilder.ApplyConfiguration(
            new Features.MaintenanceRequests.Submit.Database.WorkOrderConfiguration());
        modelBuilder.ApplyConfiguration(
            new Features.MaintenanceRequests.Submit.Database.PropertyConfiguration());
        modelBuilder.ApplyConfiguration(
            new Features.MaintenanceRequests.Submit.Database.UnitConfiguration());

        // Assign Maintenance Request slice - adds Vendor table
        modelBuilder.ApplyConfiguration(
            new Features.MaintenanceRequests.Assign.Database.VendorConfiguration());

        // Complete Work Order slice - uses same tables, no additional configs needed
        // (WorkOrder, Property, Vendor already configured above)

        // This is the ONLY place where slices "touch" - through the shared database
    }
}
