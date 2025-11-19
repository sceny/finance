using System.IO.Pipelines;

namespace Sceny.Finance.IO.Targets;

/// <summary>
/// Represents a file-based data target.
/// </summary>
public sealed class FileTarget(string filePath) : ITarget
{
    public string FilePath { get; } = ValidatePath(filePath, nameof(filePath));

    /// <inheritdoc/>
    /// <remarks>
    /// For file targets, prefer using GetPipeWriterAsync() for proper async/await support.
    /// This synchronous method will block until the file is opened.
    /// </remarks>
    public PipeWriter GetPipeWriter(CancellationToken cancellationToken = default)
    {
        // For file targets, we need to open asynchronously but the interface requires sync
        // This will block - consider using GetPipeWriterAsync() instead
        return GetPipeWriterAsync(cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets a PipeWriter asynchronously for better async/await support.
    /// </summary>
    public async Task<PipeWriter> GetPipeWriterAsync(CancellationToken cancellationToken = default)
    {
        return await Utilities.CreatePipeWriterFromFileAsync(FilePath, cancellationToken).ConfigureAwait(false);
    }

    private static string ValidatePath(string path, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path, paramName, "File path must be provided.");
        return path;
    }
}

