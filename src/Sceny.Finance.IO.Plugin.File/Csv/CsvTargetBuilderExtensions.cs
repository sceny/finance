using Sceny.Finance.IO;

namespace Sceny.Finance.IO.Plugin.File.Csv;

/// <summary>
/// Extension methods for configuring CSV format writers.
/// These methods extend TargetBuilder to provide CSV format support.
/// </summary>
public static class CsvTargetBuilderExtensions
{
    /// <summary>
    /// Configures the target to be written as CSV format with default options.
    /// </summary>
    /// <typeparam name="TTarget">The type of target</typeparam>
    /// <param name="builder">The target builder</param>
    /// <returns>A configured writer for CSV format</returns>
    public static ConfiguredWriter AsCsv<TTarget>(this TargetBuilder<TTarget> builder) where TTarget : ITarget
    {
        ArgumentNullException.ThrowIfNull(builder);

        var writer = new CsvSourceWriter();
        return new ConfiguredWriter(builder.Target, writer);
    }

    /// <summary>
    /// Configures the target to be written as CSV format with custom options.
    /// </summary>
    /// <typeparam name="TTarget">The type of target</typeparam>
    /// <param name="builder">The target builder</param>
    /// <param name="configure">Action to configure CSV options</param>
    /// <returns>A configured writer for CSV format</returns>
    public static ConfiguredWriter AsCsv<TTarget>(this TargetBuilder<TTarget> builder, Action<CsvOptions> configure) where TTarget : ITarget
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new CsvOptions();
        configure(options);
        var writer = new CsvSourceWriter(options);
        return new ConfiguredWriter(builder.Target, writer);
    }
}

