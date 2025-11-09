namespace PropertyManagement.Web.Domain.Vendors;

/// <summary>
/// Entity representing a technician working for a vendor
/// </summary>
public class Technician
{
    public Guid Id { get; private set; }
    public Guid VendorId { get; private set; }
    public string Name { get; private set; }
    public string PhoneNumber { get; private set; }
    public string Email { get; private set; }
    public bool IsAvailable { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // EF Core constructor - initializes non-nullable properties to satisfy compiler
    private Technician()
    {
        Name = null!;
        PhoneNumber = null!;
        Email = null!;
    }

    private Technician(Guid vendorId, string name, string phoneNumber, string email)
    {
        Id = Guid.NewGuid();
        VendorId = vendorId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        IsAvailable = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static Technician Create(Guid vendorId, string name, string phoneNumber, string email)
    {
        return new Technician(vendorId, name, phoneNumber, email);
    }

    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
    }
}
