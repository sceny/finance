namespace Sceny.Finance.IO;

/// <summary>
/// Represents the type of account, supporting both personal finance and trading applications.
/// </summary>
public enum AccountType
{
    /// <summary>Standard checking account</summary>
    Checking,
    
    /// <summary>Savings account</summary>
    Savings,
    
    /// <summary>Credit card account</summary>
    CreditCard,
    
    /// <summary>Brokerage account for trading</summary>
    Brokerage,
    
    /// <summary>Investment account</summary>
    Investment,
    
    /// <summary>Loan account</summary>
    Loan,
    
    /// <summary>Mortgage account</summary>
    Mortgage,
    
    /// <summary>Other/unspecified account type</summary>
    Other
}

