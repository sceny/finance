using System.IO.Pipelines;

namespace Sceny.Finance.IO.Sources;

/// <summary>
/// Represents a file-based data source.
/// </summary>
public sealed class FileSource(string filePath) : ISource
{
    public string FilePath { get; } = ValidatePath(filePath, nameof(filePath));

    /// <inheritdoc/>
    /// <remarks>
    /// For file sources, prefer using GetPipeReaderAsync() for proper async/await support.
    /// This synchronous method will block until the file is read.
    /// </remarks>
    public PipeReader GetPipeReader(CancellationToken cancellationToken = default)
    {
        // For file sources, we need to read asynchronously but the interface requires sync
        // This will block - consider using GetPipeReaderAsync() instead
        return Utilities.CreatePipeReaderFromFileAsync(FilePath, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets a PipeReader asynchronously for better async/await support.
    /// </summary>
    public async Task<PipeReader> GetPipeReaderAsync(CancellationToken cancellationToken = default)
    {
        return await Utilities.CreatePipeReaderFromFileAsync(FilePath, cancellationToken).ConfigureAwait(false);
    }

    private static string ValidatePath(string path, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path, paramName, "File path must be provided.");
        return path;
    }
}

