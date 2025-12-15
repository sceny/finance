namespace Sceny.Finance.IO.Plugin.File.Ofx;

/// <summary>
/// OFX-specific structured properties for Account.
/// Implements atomic interfaces to declare supported properties.
/// </summary>
public readonly struct OfxAccountProperties :
    IProperties,
    IPropertiesFactory<OfxAccountProperties>,
    IBankId,
    IExtended
{
    /// <summary>Bank ID from OFX</summary>
    public ReadOnlyString BankId { get; }

    /// <summary>Unstructured Extended properties</summary>
    public ReadOnlyExtended Extended { get; }

    public OfxAccountProperties(
        ReadOnlyString bankId = default,
        ReadOnlyExtended extended = default)
    {
        BankId = bankId;
        Extended = extended;
    }

    static OfxAccountProperties IPropertiesFactory<OfxAccountProperties>.FromSource<TSource>(TSource source)
    {
        return new OfxAccountProperties(
            bankId: source is IBankId bi ? bi.BankId : default,
            extended: source is IExtended ext ? ext.Extended : default
        );
    }
}




















