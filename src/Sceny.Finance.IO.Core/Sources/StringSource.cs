using System.IO.Pipelines;
using System.Text;

namespace Sceny.Finance.IO.Sources;

/// <summary>
/// Represents a string-based data source.
/// </summary>
public sealed class StringSource(string content, Encoding? encoding = null) : ISource
{
    public string Content { get; } = EnsureContent(content, nameof(content));

    public Encoding Encoding { get; } = encoding ?? Encoding.UTF8;

    /// <inheritdoc/>
    public PipeReader GetPipeReader(CancellationToken cancellationToken = default)
    {
        return Utilities.CreatePipeReaderFromString(Content, Encoding);
    }

    private static string EnsureContent(string? value, string paramName)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        return value;
    }
}

