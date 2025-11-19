using System.IO.Pipelines;
using System.Text;

namespace Sceny.Finance.IO.Targets;

/// <summary>
/// Represents a string builder-based data target.
/// </summary>
public sealed class StringTarget(StringBuilder stringBuilder, Encoding? encoding = null) : ITarget
{
    public StringBuilder StringBuilder { get; } = stringBuilder ?? throw new ArgumentNullException(nameof(stringBuilder));
    public Encoding Encoding { get; } = encoding ?? Encoding.UTF8;

    /// <inheritdoc/>
    public PipeWriter GetPipeWriter(CancellationToken cancellationToken = default)
    {
        return Utilities.CreatePipeWriterFromStringBuilder(StringBuilder, Encoding);
    }

    /// <inheritdoc/>
    public Task<PipeWriter> GetPipeWriterAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetPipeWriter(cancellationToken));
    }
}

