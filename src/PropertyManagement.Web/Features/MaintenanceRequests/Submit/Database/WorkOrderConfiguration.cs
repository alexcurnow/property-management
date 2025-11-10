using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyManagement.Web.Features.MaintenanceRequests.Submit.Models;

namespace PropertyManagement.Web.Features.MaintenanceRequests.Submit.Database;

/// <summary>
/// EF Core configuration for WorkOrder in the Submit feature
/// This configuration belongs to THIS slice only
/// </summary>
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

        builder.Property(w => w.PriorityLevel)
            .IsRequired();

        builder.Property(w => w.PriorityName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.EstimatedCostAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(w => w.EstimatedCostCurrency)
            .HasMaxLength(3);

        builder.Property(w => w.CreatedAt)
            .IsRequired();

        // Indexes for common queries
        builder.HasIndex(w => w.PropertyId);
        builder.HasIndex(w => w.CreatedAt);
        builder.HasIndex(w => w.Status);
    }
}

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

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("Units");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.UnitNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(u => new { u.PropertyId, u.UnitNumber })
            .IsUnique();
    }
}
