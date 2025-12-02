namespace Sceny.Finance.IO;

/// <summary>
/// Represents a financial account with source-specific properties.
/// Zero-allocation readonly struct using ReadOnlyString for string data.
/// </summary>
/// <typeparam name="TProperties">The source-specific properties type</typeparam>
public readonly struct Account<TProperties>
    where TProperties : struct, IProperties
{
    /// <summary>Unique account identifier</summary>
    public ReadOnlyString Id { get; }
    
    /// <summary>Display name of the account</summary>
    public ReadOnlyString Name { get; }
    
    /// <summary>Type of the account</summary>
    public AccountType Type { get; }
    
    /// <summary>ISO currency code (e.g., "USD", "EUR")</summary>
    public ReadOnlyString Currency { get; }

    /// <summary>Source-specific structured properties</summary>
    public TProperties Properties { get; }

    public Account(
        ReadOnlyString id,
        ReadOnlyString name,
        AccountType type,
        ReadOnlyString currency,
        TProperties properties)
    {
        Id = id;
        Name = name;
        Type = type;
        Currency = currency;
        Properties = properties;
    }

    /// <summary>
    /// Creates an Account from string values.
    /// Uses implicit conversion from string to ReadOnlyString.
    /// </summary>
    public static Account<TProperties> FromStrings(
        string id,
        string name,
        AccountType type,
        string currency,
        TProperties properties)
    {
        return new Account<TProperties>(id, name, type, currency, properties);
    }
}

