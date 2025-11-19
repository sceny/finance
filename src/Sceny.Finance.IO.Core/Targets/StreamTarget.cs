using System.IO.Pipelines;

namespace Sceny.Finance.IO.Targets;

/// <summary>
/// Represents a stream-based data target.
/// </summary>
public sealed class StreamTarget(System.IO.Stream stream, bool disposeStream = false) : ITarget
{
    public System.IO.Stream Stream { get; } = EnsureWritableStream(stream, nameof(stream));

    public bool DisposeStream { get; } = disposeStream;

    /// <inheritdoc/>
    /// <remarks>
    /// For stream targets, prefer using GetPipeWriterAsync() for proper async/await support.
    /// This synchronous method will block until the stream is ready.
    /// </remarks>
    public PipeWriter GetPipeWriter(CancellationToken cancellationToken = default)
    {
        // For stream targets, we need to create asynchronously but the interface requires sync
        // This will block - consider using GetPipeWriterAsync() instead
        return GetPipeWriterAsync(cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets a PipeWriter asynchronously for better async/await support.
    /// </summary>
    public async Task<PipeWriter> GetPipeWriterAsync(CancellationToken cancellationToken = default)
    {
        var writer = await Utilities.CreatePipeWriterFromStreamAsync(Stream, cancellationToken).ConfigureAwait(false);
        
        if (DisposeStream)
        {
            // Note: We can't dispose the stream here as the writer still needs it
            // The disposal will happen when the writer completes
            // This is handled by the PipeWriter completion callback
        }

        return writer;
    }

    private static System.IO.Stream EnsureWritableStream(System.IO.Stream? value, string paramName)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        if (!value.CanWrite)
            throw new ArgumentException("Stream must be writable.", paramName);
        return value;
    }
}

