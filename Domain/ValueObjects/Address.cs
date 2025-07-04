namespace Gomotel.Domain.ValueObjects;

public record Address(string Street, string City, string State, string ZipCode, string Country)
{
    public static Address Create(
        string street,
        string city,
        string state,
        string zipCode,
        string country
    )
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty", nameof(street));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be empty", nameof(state));
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("ZipCode cannot be empty", nameof(zipCode));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty", nameof(country));

        return new Address(street, city, state, zipCode, country);
    }

    public override string ToString() => $"{Street}, {City}, {State} {ZipCode}, {Country}";
}
