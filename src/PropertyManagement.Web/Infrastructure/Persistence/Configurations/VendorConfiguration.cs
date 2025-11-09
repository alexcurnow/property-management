using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyManagement.Web.Domain.Vendors;

namespace PropertyManagement.Web.Infrastructure.Persistence.Configurations;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(v => v.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Specialization)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(v => v.IsActive)
            .IsRequired();

        // Configure one-to-many relationship with Technicians
        // Using the navigation property but tell EF to use the backing field
        builder.Navigation(v => v.Technicians)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(v => v.Technicians)
            .WithOne()
            .HasForeignKey(nameof(Technician.VendorId))
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => v.Email)
            .IsUnique();
    }
}
