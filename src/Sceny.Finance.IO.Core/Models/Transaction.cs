namespace Sceny.Finance.IO;

/// <summary>
/// Represents a financial transaction. Zero-allocation readonly struct using ReadOnlyString for string data.
/// </summary>
public readonly struct Transaction
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

    public Transaction(
        ReadOnlyString accountId,
        decimal amount,
        DateTime date,
        ReadOnlyString description,
        TransactionType type,
        ReadOnlyString reference = default)
    {
        AccountId = accountId;
        Amount = amount;
        Date = date;
        Description = description;
        Type = type;
        Reference = reference;
    }

    /// <summary>
    /// Creates a Transaction from string values.
    /// Uses implicit conversion from string to ReadOnlyString.
    /// </summary>
    public static Transaction FromStrings(
        string accountId,
        decimal amount,
        DateTime date,
        string description,
        TransactionType type,
        string? reference = null)
    {
        return new Transaction(
            accountId,
            amount,
            date,
            description,
            type,
            reference ?? string.Empty
        );
    }
}

