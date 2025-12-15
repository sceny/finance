namespace Sceny.Finance.IO;

/// <summary>
/// Atomic interface for BankId property.
/// Properties structs implement this interface to declare support for BankId.
/// </summary>
public interface IBankId
{
    /// <summary>
    /// Gets the bank ID.
    /// </summary>
    ReadOnlyString BankId { get; }
}




















