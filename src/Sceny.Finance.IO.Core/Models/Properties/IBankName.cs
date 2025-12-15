namespace Sceny.Finance.IO;

/// <summary>
/// Atomic interface for BankName property.
/// Properties structs implement this interface to declare support for BankName.
/// </summary>
public interface IBankName
{
    /// <summary>
    /// Gets the bank name.
    /// </summary>
    ReadOnlyString BankName { get; }
}




















