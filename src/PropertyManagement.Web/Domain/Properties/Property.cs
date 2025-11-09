namespace PropertyManagement.Web.Domain.Properties;

/// <summary>
/// Aggregate root representing a physical property
/// </summary>
public class Property
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Address Address { get; private set; }
    public PropertyType Type { get; private set; }
    private readonly List<Unit> _units = new();
    public IReadOnlyCollection<Unit> Units => _units.AsReadOnly();
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // EF Core constructor - initializes non-nullable properties to satisfy compiler
    private Property()
    {
        Name = null!;
        Address = null!;
    }

    private Property(string name, Address address, PropertyType type)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Address = address ?? throw new ArgumentNullException(nameof(address));
        Type = type;
        CreatedAt = DateTime.UtcNow;
    }

    public static Property Create(string name, Address address, PropertyType type)
    {
        return new Property(name, address, type);
    }

    public void AddUnit(Unit unit)
    {
        if (unit == null)
            throw new ArgumentNullException(nameof(unit));

        if (_units.Any(u => u.UnitNumber == unit.UnitNumber))
            throw new InvalidOperationException($"Unit {unit.UnitNumber} already exists in this property");

        _units.Add(unit);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAddress(Address newAddress)
    {
        Address = newAddress ?? throw new ArgumentNullException(nameof(newAddress));
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum PropertyType
{
    SingleFamily,
    MultiFamily,
    Apartment,
    Commercial,
    Industrial
}
