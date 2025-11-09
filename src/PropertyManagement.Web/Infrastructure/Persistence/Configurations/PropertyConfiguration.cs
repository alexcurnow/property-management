using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyManagement.Web.Domain.Properties;

namespace PropertyManagement.Web.Infrastructure.Persistence.Configurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Type)
            .IsRequired()
            .HasConversion<string>();

        // Configure Address value object
        builder.OwnsOne(p => p.Address, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("AddressStreet")
                .IsRequired()
                .HasMaxLength(200);

            address.Property(a => a.City)
                .HasColumnName("AddressCity")
                .IsRequired()
                .HasMaxLength(100);

            address.Property(a => a.State)
                .HasColumnName("AddressState")
                .IsRequired()
                .HasMaxLength(50);

            address.Property(a => a.PostalCode)
                .HasColumnName("AddressPostalCode")
                .IsRequired()
                .HasMaxLength(20);

            address.Property(a => a.Country)
                .HasColumnName("AddressCountry")
                .IsRequired()
                .HasMaxLength(50);
        });

        // Configure one-to-many relationship with Units
        // Using the navigation property but tell EF to use the backing field
        builder.Navigation(p => p.Units)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.Units)
            .WithOne()
            .HasForeignKey(nameof(Unit.PropertyId))
            .OnDelete(DeleteBehavior.Cascade);
    }
}
