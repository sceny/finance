using System.Buffers;
using System.IO.Pipelines;

namespace Sceny.Finance.IO.Targets;

/// <summary>
/// Represents a memory-based data target (IBufferWriter).
/// </summary>
public sealed class MemoryTarget(IBufferWriter<byte> bufferWriter) : ITarget
{
    public IBufferWriter<byte> BufferWriter { get; } = bufferWriter ?? throw new ArgumentNullException(nameof(bufferWriter));

    /// <inheritdoc/>
    public PipeWriter GetPipeWriter(CancellationToken cancellationToken = default)
    {
        return Utilities.CreatePipeWriterFromBufferWriter(BufferWriter);
    }

    /// <inheritdoc/>
    public Task<PipeWriter> GetPipeWriterAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetPipeWriter(cancellationToken));
    }
}

