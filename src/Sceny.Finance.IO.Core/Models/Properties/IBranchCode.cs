namespace Sceny.Finance.IO;

/// <summary>
/// Atomic interface for BranchCode property.
/// Properties structs implement this interface to declare support for BranchCode.
/// </summary>
public interface IBranchCode
{
    /// <summary>
    /// Gets the branch code.
    /// </summary>
    ReadOnlyString BranchCode { get; }
}

















