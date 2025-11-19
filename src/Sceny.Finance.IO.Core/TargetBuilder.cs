namespace Sceny.Finance.IO;

/// <summary>
/// Base class for fluent builders that configure target writers.
/// This is format-agnostic - format-specific methods are provided via extension methods in provider projects.
/// </summary>
/// <typeparam name="TTarget">The type of target being built</typeparam>
public class TargetBuilder<TTarget>(TTarget target) where TTarget : ITarget
{
    /// <summary>
    /// Gets the target being built.
    /// </summary>
    public TTarget Target { get; } = target ?? throw new ArgumentNullException(nameof(target));
}

