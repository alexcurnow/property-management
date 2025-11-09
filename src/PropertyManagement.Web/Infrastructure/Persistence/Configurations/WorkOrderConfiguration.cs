using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyManagement.Web.Domain.WorkOrders;

namespace PropertyManagement.Web.Infrastructure.Persistence.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(w => w.CompletionNotes)
            .HasMaxLength(2000);

        // Configure value objects
        builder.OwnsOne(w => w.Status, status =>
        {
            status.Property(s => s.Value)
                .HasColumnName("Status")
                .IsRequired()
                .HasMaxLength(50);
        });

        builder.OwnsOne(w => w.Priority, priority =>
        {
            priority.Property(p => p.Level)
                .HasColumnName("PriorityLevel")
                .IsRequired();

            priority.Property(p => p.Name)
                .HasColumnName("PriorityName")
                .IsRequired()
                .HasMaxLength(50);
        });

        builder.OwnsOne(w => w.EstimatedCost, cost =>
        {
            cost.Property(c => c.Amount)
                .HasColumnName("EstimatedCostAmount")
                .HasColumnType("decimal(18,2)");

            cost.Property(c => c.Currency)
                .HasColumnName("EstimatedCostCurrency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(w => w.ActualCost, cost =>
        {
            cost.Property(c => c.Amount)
                .HasColumnName("ActualCostAmount")
                .HasColumnType("decimal(18,2)");

            cost.Property(c => c.Currency)
                .HasColumnName("ActualCostCurrency")
                .HasMaxLength(3);
        });

        // Ignore domain events collection (not persisted)
        builder.Ignore(w => w.DomainEvents);

        builder.HasIndex(w => w.PropertyId);
        builder.HasIndex(w => w.AssignedVendorId);
        builder.HasIndex(w => w.CreatedAt);
    }
}
