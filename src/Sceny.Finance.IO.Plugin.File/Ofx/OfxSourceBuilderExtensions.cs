using Sceny.Finance.IO;

namespace Sceny.Finance.IO.Plugin.File.Ofx;

/// <summary>
/// Extension methods for configuring OFX format readers.
/// These methods extend SourceBuilder to provide OFX format support.
/// </summary>
public static class OfxSourceBuilderExtensions
{
    /// <summary>
    /// Configures the source to be read as OFX format with default options.
    /// </summary>
    /// <typeparam name="TSource">The type of source</typeparam>
    /// <param name="builder">The source builder</param>
    /// <returns>A configured reader for OFX format</returns>
    public static ConfiguredReader AsOfx<TSource>(this SourceBuilder<TSource> builder) where TSource : ISource
    {
        ArgumentNullException.ThrowIfNull(builder);

        var reader = new OfxSourceReader();
        return new ConfiguredReader(builder.Source, reader);
    }

    /// <summary>
    /// Configures the source to be read as OFX format with custom options.
    /// </summary>
    /// <typeparam name="TSource">The type of source</typeparam>
    /// <param name="builder">The source builder</param>
    /// <param name="configure">Action to configure OFX options</param>
    /// <returns>A configured reader for OFX format</returns>
    public static ConfiguredReader AsOfx<TSource>(this SourceBuilder<TSource> builder, Action<OfxOptions> configure) where TSource : ISource
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new OfxOptions();
        configure(options);
        var reader = new OfxSourceReader(options);
        return new ConfiguredReader(builder.Source, reader);
    }
}

