using System.IO.Pipelines;

namespace Sceny.Finance.IO;

/// <summary>
/// Represents a target destination that can provide a PipeWriter for streaming writes.
/// This abstraction allows different data destinations (files, streams, HTTP, SignalR, etc.) to be used uniformly.
/// </summary>
public interface ITarget
{
    /// <summary>
    /// Gets a PipeWriter for the data target.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A PipeWriter that can be used to write to the target</returns>
    PipeWriter GetPipeWriter(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a PipeWriter asynchronously for the data target.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A PipeWriter that can be used to write to the target</returns>
    Task<PipeWriter> GetPipeWriterAsync(CancellationToken cancellationToken = default);
}

