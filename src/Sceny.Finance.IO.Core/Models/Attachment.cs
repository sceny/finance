namespace Sceny.Finance.IO;

/// <summary>
/// Represents a binary attachment with metadata.
/// Zero-allocation readonly struct using ReadOnlyMemory&lt;byte&gt; for binary data.
/// </summary>
public readonly struct Attachment
{
    /// <summary>Name of the attachment</summary>
    public ReadOnlyString Name { get; }

    /// <summary>MIME content type (e.g., "image/png", "application/pdf")</summary>
    public ReadOnlyString ContentType { get; }

    /// <summary>Binary data of the attachment</summary>
    public ReadOnlyMemory<byte> Data { get; }

    /// <summary>Optional reference or URI to the attachment</summary>
    public ReadOnlyString Reference { get; }

    public Attachment(
        ReadOnlyString name,
        ReadOnlyString contentType,
        ReadOnlyMemory<byte> data,
        ReadOnlyString reference = default)
    {
        Name = name;
        ContentType = contentType;
        Data = data;
        Reference = reference;
    }

    /// <summary>
    /// Gets whether the attachment has data.
    /// </summary>
    public bool HasData => !Data.IsEmpty;

    /// <summary>
    /// Gets the size of the attachment data in bytes.
    /// </summary>
    public int Size => Data.Length;
}

