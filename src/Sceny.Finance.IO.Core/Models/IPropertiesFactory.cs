namespace Sceny.Finance.IO;

/// <summary>
/// Factory interface for zero-allocation generic property mapping.
/// Each Properties struct implements this interface with pattern matching logic
/// to map from any source Properties type to its own structure.
/// </summary>
/// <typeparam name="T">The target Properties type</typeparam>
public interface IPropertiesFactory<T> where T : struct, IProperties
{
    /// <summary>
    /// Creates an instance of T from a source Properties type using pattern matching.
    /// Zero-allocation mapping that extracts available properties via atomic interfaces.
    /// </summary>
    /// <typeparam name="TSource">The source Properties type</typeparam>
    /// <param name="source">The source Properties instance</param>
    /// <returns>A new instance of T with properties mapped from source</returns>
    static abstract T FromSource<TSource>(TSource source) where TSource : struct, IProperties;
}

