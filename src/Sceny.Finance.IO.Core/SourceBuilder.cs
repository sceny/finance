namespace Sceny.Finance.IO;

/// <summary>
/// Base class for fluent builders that configure source readers.
/// This is format-agnostic - format-specific methods are provided via extension methods in provider projects.
/// </summary>
/// <typeparam name="TSource">The type of source being built</typeparam>
public class SourceBuilder<TSource>(TSource source) where TSource : ISource
{
    /// <summary>
    /// Gets the source being built.
    /// </summary>
    public TSource Source { get; } = source ?? throw new ArgumentNullException(nameof(source));
}

