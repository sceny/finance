using System.IO.Pipelines;
using Sceny.Finance.IO.Sources;
using Sceny.Finance.IO.Targets;

namespace Sceny.Finance.IO;

/// <summary>
/// Extension methods for fluent copy operations between sources and targets.
/// Provides zero-allocation piped copying with a developer-friendly API.
/// </summary>
public static class CopyExtensions
{
    /// <summary>
    /// Copies data from a SourceBuilder to a TargetBuilder in a zero-allocation way using pipes.
    /// This is a one-hit fluent setup for copying between any source and target.
    /// </summary>
    /// <typeparam name="TSource">The type of source</typeparam>
    /// <typeparam name="TTarget">The type of target</typeparam>
    /// <param name="sourceBuilder">The source builder to read from</param>
    /// <param name="targetBuilder">The target builder to write to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the copy operation</returns>
    public static async Task CopyToAsync<TSource, TTarget>(
        this SourceBuilder<TSource> sourceBuilder,
        TargetBuilder<TTarget> targetBuilder,
        CancellationToken cancellationToken = default)
        where TSource : ISource
        where TTarget : ITarget
    {
        ArgumentNullException.ThrowIfNull(sourceBuilder);
        ArgumentNullException.ThrowIfNull(targetBuilder);

        await sourceBuilder.Source.CopyToAsync(targetBuilder.Target, cancellationToken).ConfigureAwait(false);
    }
    /// <summary>
    /// Copies data from a source to a target in a zero-allocation way using pipes.
    /// This is a one-hit fluent setup for copying between any source and target.
    /// </summary>
    /// <param name="source">The source to read from</param>
    /// <param name="target">The target to write to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the copy operation</returns>
    public static async Task CopyToAsync(
        this ISource source,
        ITarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        PipeReader reader;
        PipeWriter writer;

        // Get reader from source
        if (source is FileSource fileSource)
        {
            reader = await fileSource.GetPipeReaderAsync(cancellationToken).ConfigureAwait(false);
        }
        else if (source is StreamSource streamSource)
        {
            reader = await streamSource.GetPipeReaderAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            reader = source.GetPipeReader(cancellationToken);
        }

        // Get writer from target
        if (target is FileTarget fileTarget)
        {
            writer = await fileTarget.GetPipeWriterAsync(cancellationToken).ConfigureAwait(false);
        }
        else if (target is StreamTarget streamTarget)
        {
            writer = await streamTarget.GetPipeWriterAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            writer = await target.GetPipeWriterAsync(cancellationToken).ConfigureAwait(false);
        }

        // Copy using zero-allocation pipe operation
        await Utilities.CopyPipeAsync(reader, writer, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a PipeReader from a source for manual zero-allocation copying.
    /// </summary>
    /// <param name="source">The source to get a reader from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A PipeReader for manual copying</returns>
    public static async Task<PipeReader> GetPipeReaderAsync(
        this ISource source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source is FileSource fileSource)
        {
            return await fileSource.GetPipeReaderAsync(cancellationToken).ConfigureAwait(false);
        }
        else if (source is StreamSource streamSource)
        {
            return await streamSource.GetPipeReaderAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            return source.GetPipeReader(cancellationToken);
        }
    }

    /// <summary>
    /// Gets a PipeWriter from a target for manual zero-allocation copying.
    /// </summary>
    /// <param name="target">The target to get a writer from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A PipeWriter for manual copying</returns>
    public static async Task<PipeWriter> GetPipeWriterAsync(
        this ITarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (target is FileTarget fileTarget)
        {
            return await fileTarget.GetPipeWriterAsync(cancellationToken).ConfigureAwait(false);
        }
        else if (target is StreamTarget streamTarget)
        {
            return await streamTarget.GetPipeWriterAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            return await target.GetPipeWriterAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}

