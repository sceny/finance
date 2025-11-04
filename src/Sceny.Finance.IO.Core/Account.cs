namespace Sceny.Finance.IO;

/// <summary>
/// Represents a financial account. Zero-allocation readonly struct using ReadOnlyString for string data.
/// </summary>
public readonly struct Account
{
    /// <summary>Unique account identifier</summary>
    public ReadOnlyString Id { get; }
    
    /// <summary>Display name of the account</summary>
    public ReadOnlyString Name { get; }
    
    /// <summary>Type of the account</summary>
    public AccountType Type { get; }
    
    /// <summary>ISO currency code (e.g., "USD", "EUR")</summary>
    public ReadOnlyString Currency { get; }

    public Account(ReadOnlyString id, ReadOnlyString name, AccountType type, ReadOnlyString currency)
    {
        Id = id;
        Name = name;
        Type = type;
        Currency = currency;
    }

    /// <summary>
    /// Creates an Account from string values.
    /// Uses implicit conversion from string to ReadOnlyString.
    /// </summary>
    public static Account FromStrings(string id, string name, AccountType type, string currency)
    {
        return new Account(id, name, type, currency);
    }
}
