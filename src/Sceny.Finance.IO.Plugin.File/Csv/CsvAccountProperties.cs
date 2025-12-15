namespace Sceny.Finance.IO.Plugin.File.Csv;

/// <summary>
/// CSV-specific structured properties for Account.
/// Implements atomic interfaces to declare supported properties.
/// </summary>
public readonly struct CsvAccountProperties :
    IProperties,
    IPropertiesFactory<CsvAccountProperties>,
    IBankName,
    IBranchCode,
    IExtended,
    IAttachments
{
    /// <summary>Bank name from CSV</summary>
    public ReadOnlyString BankName { get; }

    /// <summary>Branch code from CSV</summary>
    public ReadOnlyString BranchCode { get; }

    /// <summary>Unstructured Extended properties</summary>
    public ReadOnlyExtended Extended { get; }

    /// <summary>Attachments collection</summary>
    public ReadOnlyAttachments Attachments { get; }

    public CsvAccountProperties(
        ReadOnlyString bankName = default,
        ReadOnlyString branchCode = default,
        ReadOnlyExtended extended = default,
        ReadOnlyAttachments attachments = default)
    {
        BankName = bankName;
        BranchCode = branchCode;
        Extended = extended;
        Attachments = attachments;
    }

    static CsvAccountProperties IPropertiesFactory<CsvAccountProperties>.FromSource<TSource>(TSource source)
    {
        return new CsvAccountProperties(
            bankName: source is IBankName bn ? bn.BankName : default,
            branchCode: source is IBranchCode bc ? bc.BranchCode : default,
            extended: source is IExtended ext ? ext.Extended : default,
            attachments: source is IAttachments att ? att.Attachments : default
        );
    }
}




















