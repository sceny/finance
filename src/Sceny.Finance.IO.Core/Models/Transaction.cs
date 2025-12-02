namespace Sceny.Finance.IO;

/// <summary>
/// Represents a financial transaction with source-specific properties.
/// Zero-allocation readonly struct using ReadOnlyString for string data.
/// </summary>
/// <typeparam name="TProperties">The source-specific properties type</typeparam>
public readonly struct Transaction<TProperties>
    where TProperties : struct, IProperties
{
    /// <summary>Reference to the source account ID</summary>
    public ReadOnlyString AccountId { get; }
    
    /// <summary>Transaction amount (positive = credit/deposit, negative = debit/withdrawal)</summary>
    public decimal Amount { get; }
    
    /// <summary>Transaction date</summary>
    public DateTime Date { get; }
    
    /// <summary>Transaction description/memo</summary>
    public ReadOnlyString Description { get; }
    
    /// <summary>Type of transaction</summary>
    public TransactionType Type { get; }
    
    /// <summary>Optional transaction reference/check number</summary>
    public ReadOnlyString Reference { get; }

    /// <summary>Source-specific structured properties</summary>
    public TProperties Properties { get; }

    public Transaction(
        ReadOnlyString accountId,
        decimal amount,
        DateTime date,
        ReadOnlyString description,
        TransactionType type,
        TProperties properties,
        ReadOnlyString reference = default)
    {
        AccountId = accountId;
        Amount = amount;
        Date = date;
        Description = description;
        Type = type;
        Reference = reference;
        Properties = properties;
    }

    /// <summary>
    /// Creates a Transaction from string values.
    /// Uses implicit conversion from string to ReadOnlyString.
    /// </summary>
    public static Transaction<TProperties> FromStrings(
        string accountId,
        decimal amount,
        DateTime date,
        string description,
        TransactionType type,
        TProperties properties,
        string? reference = null)
    {
        return new Transaction<TProperties>(
            accountId,
            amount,
            date,
            description,
            type,
            properties,
            reference ?? string.Empty
        );
    }
}

