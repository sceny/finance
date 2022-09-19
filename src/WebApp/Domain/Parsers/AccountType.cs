namespace WebApp.Domain.Parsers;

public enum AccountType
{
    /// <summary>
    /// !Type:Cash	Cash Flow: Cash Account
    /// </summary>
    Cash,
    /// <summary>
    /// !Type:Bank	Cash Flow: Checking & Savings Account
    /// </summary>
    Bank,
    /// <summary>
    /// !Type:CCard	Cash Flow: Credit Card Account
    /// </summary>
    CreditCard,
    /// <summary>
    /// !Type:Invst	Investing: Investment Account
    /// </summary>
    Investment,
    /// <summary>
    /// !Type:Oth A	Property & Debt: Asset
    /// </summary>
    Asset,
    /// <summary>
    /// !Type:Oth L	Property & Debt: Liability
    /// </summary>
    Liability,
    /// <summary>
    /// !Type:Invoice	Invoice (Quicken for Business only)
    /// </summary>
    Invoice,
    Unknown
}