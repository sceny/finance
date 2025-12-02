namespace Sceny.Finance.IO;

/// <summary>
/// Utilities for zero-allocation generic property mapping between different source types.
/// Uses IPropertiesFactory pattern for type-safe, reflection-free mapping.
/// </summary>
public static class PropertyMapper
{
    /// <summary>
    /// Maps an Account from one Properties type to another using IPropertiesFactory.
    /// Zero-allocation mapping that extracts available properties via atomic interfaces.
    /// </summary>
    /// <typeparam name="TSourceProperties">The source Properties type</typeparam>
    /// <typeparam name="TTargetProperties">The target Properties type</typeparam>
    /// <param name="source">The source Account</param>
    /// <returns>A new Account with target Properties type</returns>
    public static Account<TTargetProperties> MapProperties<TSourceProperties, TTargetProperties>(
        Account<TSourceProperties> source)
        where TSourceProperties : struct, IProperties
        where TTargetProperties : struct, IProperties, IPropertiesFactory<TTargetProperties>
    {
        var targetProperties = TTargetProperties.FromSource<TSourceProperties>(source.Properties);
        return new Account<TTargetProperties>(
            source.Id,
            source.Name,
            source.Type,
            source.Currency,
            targetProperties);
    }

    /// <summary>
    /// Maps a Transaction from one Properties type to another using IPropertiesFactory.
    /// Zero-allocation mapping that extracts available properties via atomic interfaces.
    /// </summary>
    /// <typeparam name="TSourceProperties">The source Properties type</typeparam>
    /// <typeparam name="TTargetProperties">The target Properties type</typeparam>
    /// <param name="source">The source Transaction</param>
    /// <returns>A new Transaction with target Properties type</returns>
    public static Transaction<TTargetProperties> MapProperties<TSourceProperties, TTargetProperties>(
        Transaction<TSourceProperties> source)
        where TSourceProperties : struct, IProperties
        where TTargetProperties : struct, IProperties, IPropertiesFactory<TTargetProperties>
    {
        var targetProperties = TTargetProperties.FromSource<TSourceProperties>(source.Properties);
        return new Transaction<TTargetProperties>(
            source.AccountId,
            source.Amount,
            source.Date,
            source.Description,
            source.Type,
            targetProperties,
            source.Reference);
    }
}

