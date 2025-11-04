using Sceny.Finance.IO;

namespace Sceny.Finance.IO.Plugin.File.Csv;

/// <summary>
/// Extension methods for configuring CSV format readers.
/// These methods extend SourceBuilder to provide CSV format support.
/// </summary>
public static class CsvSourceBuilderExtensions
{
    /// <summary>
    /// Configures the source to be read as CSV format with default options.
    /// </summary>
    /// <typeparam name="TSource">The type of source</typeparam>
    /// <param name="builder">The source builder</param>
    /// <returns>A configured reader for CSV format</returns>
    public static ConfiguredReader AsCsv<TSource>(this SourceBuilder<TSource> builder) where TSource : ISource
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        var reader = new CsvSourceReader();
        return new ConfiguredReader(builder.Source, reader);
    }

    /// <summary>
    /// Configures the source to be read as CSV format with custom options.
    /// </summary>
    /// <typeparam name="TSource">The type of source</typeparam>
    /// <param name="builder">The source builder</param>
    /// <param name="configure">Action to configure CSV options</param>
    /// <returns>A configured reader for CSV format</returns>
    public static ConfiguredReader AsCsv<TSource>(this SourceBuilder<TSource> builder, Action<CsvOptions> configure) where TSource : ISource
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        var options = new CsvOptions();
        configure(options);
        var reader = new CsvSourceReader(options);
        return new ConfiguredReader(builder.Source, reader);
    }
}

