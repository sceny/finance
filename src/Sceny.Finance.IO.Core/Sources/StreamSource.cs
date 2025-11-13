using System.IO.Pipelines;

namespace Sceny.Finance.IO.Sources;

/// <summary>
/// Represents a stream-based data source.
/// </summary>
public sealed class StreamSource(System.IO.Stream stream, bool disposeStream = false) : ISource
{
    public System.IO.Stream Stream { get; } = EnsureReadableStream(stream, nameof(stream));

    public bool DisposeStream { get; } = disposeStream;

    /// <inheritdoc/>
    /// <remarks>
    /// For stream sources, prefer using GetPipeReaderAsync() for proper async/await support.
    /// This synchronous method will block until the stream is read.
    /// </remarks>
    public PipeReader GetPipeReader(CancellationToken cancellationToken = default)
    {
        // For stream sources, we need to read asynchronously but the interface requires sync
        // This will block - consider using GetPipeReaderAsync() instead
        return Utilities.CreatePipeReaderFromStreamAsync(Stream, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets a PipeReader asynchronously for better async/await support.
    /// </summary>
    public async Task<PipeReader> GetPipeReaderAsync(CancellationToken cancellationToken = default)
    {
        var reader = await Utilities.CreatePipeReaderFromStreamAsync(Stream, cancellationToken).ConfigureAwait(false);
        
        if (DisposeStream)
        {
            // Dispose the stream after creating the reader
            await Stream.DisposeAsync().ConfigureAwait(false);
        }

        return reader;
    }

    private static System.IO.Stream EnsureReadableStream(System.IO.Stream? value, string paramName)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        if (!value.CanRead)
            throw new ArgumentException("Stream must be readable.", paramName);
        return value;
    }
}

