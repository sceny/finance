namespace Sceny.Finance.IO;

/// <summary>
/// Atomic interface for Extended (unstructured) properties.
/// Properties structs implement this interface to declare support for unstructured key-value pairs.
/// </summary>
public interface IExtended
{
    /// <summary>
    /// Gets the Extended (unstructured) key-value pairs.
    /// </summary>
    ReadOnlyExtended Extended { get; }
}

