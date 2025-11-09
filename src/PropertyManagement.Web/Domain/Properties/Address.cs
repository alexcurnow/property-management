namespace PropertyManagement.Web.Domain.Properties;

/// <summary>
/// Value object representing a physical address
/// </summary>
public record Address
{
    public string Street { get; init; }
    public string City { get; init; }
    public string State { get; init; }
    public string PostalCode { get; init; }
    public string Country { get; init; }

    private Address(string street, string city, string state, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty", nameof(street));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be empty", nameof(state));
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code cannot be empty", nameof(postalCode));

        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country ?? "USA";
    }

    public static Address Create(string street, string city, string state, string postalCode, string country = "USA")
        => new(street, city, state, postalCode, country);

    public override string ToString() => $"{Street}, {City}, {State} {PostalCode}";
}
