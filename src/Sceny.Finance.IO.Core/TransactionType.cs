namespace Sceny.Finance.IO;

/// <summary>
/// Represents the type of transaction, supporting both personal finance and trading applications.
/// </summary>
public enum TransactionType
{
    /// <summary>Debit transaction (outgoing payment)</summary>
    Debit,
    
    /// <summary>Credit transaction (incoming payment)</summary>
    Credit,
    
    /// <summary>Transfer between accounts</summary>
    Transfer,
    
    /// <summary>Buy transaction (trading)</summary>
    Buy,
    
    /// <summary>Sell transaction (trading)</summary>
    Sell,
    
    /// <summary>Dividend payment</summary>
    Dividend,
    
    /// <summary>Fee charge</summary>
    Fee,
    
    /// <summary>Interest payment</summary>
    Interest,
    
    /// <summary>Adjustment/correction</summary>
    Adjustment,
    
    /// <summary>Other/unspecified transaction type</summary>
    Other
}

