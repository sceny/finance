using System.IO.Pipelines;

namespace Sceny.Finance.IO.Sources;

/// <summary>
/// Represents a memory-based data source (byte array or ReadOnlyMemory).
/// </summary>
public sealed class MemorySource : ISource
{
    private readonly ReadOnlyMemory<byte> _data;

    /// <summary>
    /// Creates a new MemorySource for the specified byte data.
    /// </summary>
    /// <param name="data">The byte data</param>
    public MemorySource(ReadOnlyMemory<byte> data)
    {
        _data = data;
    }

    /// <inheritdoc/>
    public PipeReader GetPipeReader(CancellationToken cancellationToken = default)
    {
        return Utilities.CreatePipeReaderFromBytes(_data);
    }
}

