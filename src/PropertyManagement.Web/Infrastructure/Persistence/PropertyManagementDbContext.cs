using Microsoft.EntityFrameworkCore;
using PropertyManagement.Web.Domain.Properties;
using PropertyManagement.Web.Domain.Scheduling;
using PropertyManagement.Web.Domain.Vendors;
using PropertyManagement.Web.Domain.WorkOrders;

namespace PropertyManagement.Web.Infrastructure.Persistence;

public class PropertyManagementDbContext : DbContext
{
    public PropertyManagementDbContext(DbContextOptions<PropertyManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Technician> Technicians => Set<Technician>();
    public DbSet<MaintenanceSchedule> MaintenanceSchedules => Set<MaintenanceSchedule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PropertyManagementDbContext).Assembly);
    }
}
