using System.IO.Pipelines;

namespace Sceny.Finance.IO.Sources;

/// <summary>
/// Represents a stream-based data source.
/// </summary>
public sealed class StreamSource : ISource
{
    private readonly System.IO.Stream _stream;
    private readonly bool _disposeStream;

    /// <summary>
    /// Creates a new StreamSource for the specified stream.
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <param name="disposeStream">Whether to dispose the stream when done (default: false)</param>
    public StreamSource(System.IO.Stream stream, bool disposeStream = false)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _disposeStream = disposeStream;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// For stream sources, prefer using GetPipeReaderAsync() for proper async/await support.
    /// This synchronous method will block until the stream is read.
    /// </remarks>
    public PipeReader GetPipeReader(CancellationToken cancellationToken = default)
    {
        // For stream sources, we need to read asynchronously but the interface requires sync
        // This will block - consider using GetPipeReaderAsync() instead
        return Utilities.CreatePipeReaderFromStreamAsync(_stream, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets a PipeReader asynchronously for better async/await support.
    /// </summary>
    public async Task<PipeReader> GetPipeReaderAsync(CancellationToken cancellationToken = default)
    {
        var reader = await Utilities.CreatePipeReaderFromStreamAsync(_stream, cancellationToken).ConfigureAwait(false);
        
        if (_disposeStream)
        {
            // Dispose the stream after creating the reader
            await _stream.DisposeAsync().ConfigureAwait(false);
        }

        return reader;
    }
}

