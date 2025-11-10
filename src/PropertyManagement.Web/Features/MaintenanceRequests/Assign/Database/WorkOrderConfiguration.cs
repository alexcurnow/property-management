using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyManagement.Web.Features.MaintenanceRequests.Assign.Models;

namespace PropertyManagement.Web.Features.MaintenanceRequests.Assign.Database;

// ============================================================================
// WORK ORDER CONFIGURATION - Assign slice
// ============================================================================

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("WorkOrders");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(w => w.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.PriorityName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.EstimatedCostCurrency)
            .HasMaxLength(3);

        builder.Property(w => w.CreatedAt)
            .IsRequired();

        // Assignment-specific indexes
        builder.HasIndex(w => w.AssignedVendorId);
        builder.HasIndex(w => new { w.Status, w.AssignedVendorId });

        // Navigation
        builder.HasOne(w => w.Vendor)
            .WithMany()
            .HasForeignKey(w => w.AssignedVendorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(w => w.Property)
            .WithMany()
            .HasForeignKey(w => w.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Use field access to avoid navigation property initialization issues
        builder.Navigation(w => w.Vendor).AutoInclude(false);
        builder.Navigation(w => w.Property).AutoInclude(false);
    }
}

// ============================================================================
// VENDOR CONFIGURATION - Assign slice
// ============================================================================

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendors");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.ContactEmail)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.ContactPhone)
            .HasMaxLength(50);

        builder.Property(v => v.Specialization)
            .HasMaxLength(200);

        // Indexes for querying
        builder.HasIndex(v => v.IsActive);
        builder.HasIndex(v => v.Specialization);
    }
}

// ============================================================================
// PROPERTY CONFIGURATION - Assign slice (minimal)
// ============================================================================

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
    }
}
