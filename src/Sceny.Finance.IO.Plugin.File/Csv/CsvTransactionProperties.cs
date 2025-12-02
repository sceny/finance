namespace Sceny.Finance.IO.Plugin.File.Csv;

/// <summary>
/// CSV-specific structured properties for Transaction.
/// Implements atomic interfaces to declare supported properties.
/// </summary>
public readonly struct CsvTransactionProperties :
    IProperties,
    IPropertiesFactory<CsvTransactionProperties>,
    IExtended,
    IAttachments
{
    /// <summary>Unstructured Extended properties</summary>
    public ReadOnlyExtended Extended { get; }

    /// <summary>Attachments collection</summary>
    public ReadOnlyAttachments Attachments { get; }

    public CsvTransactionProperties(
        ReadOnlyExtended extended = default,
        ReadOnlyAttachments attachments = default)
    {
        Extended = extended;
        Attachments = attachments;
    }

    static CsvTransactionProperties IPropertiesFactory<CsvTransactionProperties>.FromSource<TSource>(TSource source)
    {
        return new CsvTransactionProperties(
            extended: source is IExtended ext ? ext.Extended : default,
            attachments: source is IAttachments att ? att.Attachments : default
        );
    }
}

