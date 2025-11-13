using System.IO.Pipelines;

namespace Sceny.Finance.IO.Sources;

/// <summary>
/// Represents a memory-based data source (byte array or ReadOnlyMemory).
/// </summary>
public sealed class MemorySource(ReadOnlyMemory<byte> data) : ISource
{
    public ReadOnlyMemory<byte> Data { get; } = data.Length == 0 ? ReadOnlyMemory<byte>.Empty : data;

    /// <inheritdoc/>
    public PipeReader GetPipeReader(CancellationToken cancellationToken = default)
    {
        return Utilities.CreatePipeReaderFromBytes(Data);
    }
}

