namespace PropertyManagement.Web.Domain.Vendors;

/// <summary>
/// Aggregate root representing a vendor/service provider
/// </summary>
public class Vendor
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string CompanyName { get; private set; }
    public string PhoneNumber { get; private set; }
    public string Email { get; private set; }
    public VendorSpecialization Specialization { get; private set; }
    public bool IsActive { get; private set; }
    private readonly List<Technician> _technicians = new();
    public IReadOnlyCollection<Technician> Technicians => _technicians.AsReadOnly();
    public DateTime CreatedAt { get; private set; }

    // EF Core constructor - initializes non-nullable properties to satisfy compiler
    private Vendor()
    {
        Name = null!;
        CompanyName = null!;
        PhoneNumber = null!;
        Email = null!;
    }

    private Vendor(string name, string companyName, string phoneNumber, string email, VendorSpecialization specialization)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        CompanyName = companyName ?? throw new ArgumentNullException(nameof(companyName));
        PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Specialization = specialization;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static Vendor Create(string name, string companyName, string phoneNumber, string email, VendorSpecialization specialization)
    {
        return new Vendor(name, companyName, phoneNumber, email, specialization);
    }

    public void AddTechnician(Technician technician)
    {
        if (technician == null)
            throw new ArgumentNullException(nameof(technician));

        _technicians.Add(technician);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public bool CanHandleWorkOrder(VendorSpecialization requiredSpecialization)
    {
        return IsActive && Specialization == requiredSpecialization;
    }
}

public enum VendorSpecialization
{
    Plumbing,
    Electrical,
    HVAC,
    Carpentry,
    Painting,
    Landscaping,
    GeneralMaintenance,
    Appliance,
    Roofing
}
