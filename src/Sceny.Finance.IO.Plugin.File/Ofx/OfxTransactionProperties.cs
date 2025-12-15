namespace Sceny.Finance.IO.Plugin.File.Ofx;

/// <summary>
/// OFX-specific structured properties for Transaction.
/// Implements atomic interfaces to declare supported properties.
/// </summary>
public readonly struct OfxTransactionProperties :
    IProperties,
    IPropertiesFactory<OfxTransactionProperties>,
    IExtended
{
    /// <summary>Unstructured Extended properties</summary>
    public ReadOnlyExtended Extended { get; }

    public OfxTransactionProperties(ReadOnlyExtended extended = default)
    {
        Extended = extended;
    }

    static OfxTransactionProperties IPropertiesFactory<OfxTransactionProperties>.FromSource<TSource>(TSource source)
    {
        return new OfxTransactionProperties(
            extended: source is IExtended ext ? ext.Extended : default
        );
    }
}




















