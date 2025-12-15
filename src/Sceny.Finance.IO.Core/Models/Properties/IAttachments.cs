namespace Sceny.Finance.IO;

/// <summary>
/// Atomic interface for Attachments.
/// Properties structs implement this interface to declare support for binary attachments.
/// </summary>
public interface IAttachments
{
    /// <summary>
    /// Gets the Attachments collection.
    /// </summary>
    ReadOnlyAttachments Attachments { get; }
}




















