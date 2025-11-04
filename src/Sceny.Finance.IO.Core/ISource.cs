using System.IO.Pipelines;

namespace Sceny.Finance.IO;

/// <summary>
/// Represents a source of data that can provide a PipeReader for streaming consumption.
/// This abstraction allows different data sources (files, streams, HTTP, SignalR, etc.) to be used uniformly.
/// </summary>
public interface ISource
{
    /// <summary>
    /// Gets a PipeReader for the data source.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A PipeReader that can be used to read the source data</returns>
    PipeReader GetPipeReader(CancellationToken cancellationToken = default);
}

