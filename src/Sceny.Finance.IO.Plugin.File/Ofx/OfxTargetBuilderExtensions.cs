using Sceny.Finance.IO;

namespace Sceny.Finance.IO.Plugin.File.Ofx;

/// <summary>
/// Extension methods for configuring OFX format writers.
/// These methods extend TargetBuilder to provide OFX format support.
/// </summary>
public static class OfxTargetBuilderExtensions
{
    /// <summary>
    /// Configures the target to be written as OFX format with default options.
    /// </summary>
    /// <typeparam name="TTarget">The type of target</typeparam>
    /// <param name="builder">The target builder</param>
    /// <returns>A configured writer for OFX format</returns>
    public static ConfiguredWriter AsOfx<TTarget>(this TargetBuilder<TTarget> builder) where TTarget : ITarget
    {
        ArgumentNullException.ThrowIfNull(builder);

        var writer = new OfxSourceWriter();
        return new ConfiguredWriter(builder.Target, writer);
    }

    /// <summary>
    /// Configures the target to be written as OFX format with custom options.
    /// </summary>
    /// <typeparam name="TTarget">The type of target</typeparam>
    /// <param name="builder">The target builder</param>
    /// <param name="configure">Action to configure OFX options</param>
    /// <returns>A configured writer for OFX format</returns>
    public static ConfiguredWriter AsOfx<TTarget>(this TargetBuilder<TTarget> builder, Action<OfxOptions> configure) where TTarget : ITarget
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new OfxOptions();
        configure(options);
        var writer = new OfxSourceWriter(options);
        return new ConfiguredWriter(builder.Target, writer);
    }
}

