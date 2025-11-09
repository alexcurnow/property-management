namespace PropertyManagement.Web.Domain.Properties;

/// <summary>
/// Entity representing a unit within a property
/// </summary>
public class Unit
{
    public Guid Id { get; private set; }
    public Guid PropertyId { get; private set; }
    public string UnitNumber { get; private set; }
    public int Bedrooms { get; private set; }
    public decimal SquareFeet { get; private set; }
    public bool IsOccupied { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // EF Core constructor - initializes non-nullable properties to satisfy compiler
    private Unit()
    {
        UnitNumber = null!;
    }

    private Unit(Guid propertyId, string unitNumber, int bedrooms, decimal squareFeet)
    {
        Id = Guid.NewGuid();
        PropertyId = propertyId;
        UnitNumber = unitNumber ?? throw new ArgumentNullException(nameof(unitNumber));
        Bedrooms = bedrooms >= 0 ? bedrooms : throw new ArgumentException("Bedrooms cannot be negative");
        SquareFeet = squareFeet > 0 ? squareFeet : throw new ArgumentException("Square feet must be positive");
        IsOccupied = false;
        CreatedAt = DateTime.UtcNow;
    }

    public static Unit Create(Guid propertyId, string unitNumber, int bedrooms, decimal squareFeet)
    {
        return new Unit(propertyId, unitNumber, bedrooms, squareFeet);
    }

    public void SetOccupancy(bool isOccupied)
    {
        IsOccupied = isOccupied;
    }
}
