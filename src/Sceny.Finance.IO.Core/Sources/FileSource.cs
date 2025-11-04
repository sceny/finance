using System.IO.Pipelines;

namespace Sceny.Finance.IO.Sources;

/// <summary>
/// Represents a file-based data source.
/// </summary>
public sealed class FileSource : ISource
{
    private readonly string _filePath;

    /// <summary>
    /// Creates a new FileSource for the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the file</param>
    public FileSource(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// For file sources, prefer using GetPipeReaderAsync() for proper async/await support.
    /// This synchronous method will block until the file is read.
    /// </remarks>
    public PipeReader GetPipeReader(CancellationToken cancellationToken = default)
    {
        // For file sources, we need to read asynchronously but the interface requires sync
        // This will block - consider using GetPipeReaderAsync() instead
        return Utilities.CreatePipeReaderFromFileAsync(_filePath, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets a PipeReader asynchronously for better async/await support.
    /// </summary>
    public async Task<PipeReader> GetPipeReaderAsync(CancellationToken cancellationToken = default)
    {
        return await Utilities.CreatePipeReaderFromFileAsync(_filePath, cancellationToken).ConfigureAwait(false);
    }
}

