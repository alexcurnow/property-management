using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyManagement.Web.Domain.Scheduling;

namespace PropertyManagement.Web.Infrastructure.Persistence.Configurations;

public class MaintenanceScheduleConfiguration : IEntityTypeConfiguration<MaintenanceSchedule>
{
    public void Configure(EntityTypeBuilder<MaintenanceSchedule> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.TaskDescription)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.Recurrence)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(m => m.RequiredSpecialization)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(m => m.NextScheduledDate)
            .IsRequired();

        builder.Property(m => m.IsActive)
            .IsRequired();

        builder.HasIndex(m => m.PropertyId);
        builder.HasIndex(m => m.NextScheduledDate);
    }
}
