using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyManagement.Web.Domain.Properties;

namespace PropertyManagement.Web.Infrastructure.Persistence.Configurations;

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.UnitNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Bedrooms)
            .IsRequired();

        builder.Property(u => u.SquareFeet)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(u => u.IsOccupied)
            .IsRequired();

        builder.HasIndex(u => new { u.PropertyId, u.UnitNumber })
            .IsUnique();
    }
}
