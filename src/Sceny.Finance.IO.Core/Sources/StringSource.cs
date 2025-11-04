using System.IO.Pipelines;
using System.Text;

namespace Sceny.Finance.IO.Sources;

/// <summary>
/// Represents a string-based data source.
/// </summary>
public sealed class StringSource : ISource
{
    private readonly string _content;
    private readonly Encoding _encoding;

    /// <summary>
    /// Creates a new StringSource for the specified string content.
    /// </summary>
    /// <param name="content">The string content</param>
    /// <param name="encoding">The encoding to use (default: UTF-8)</param>
    public StringSource(string content, Encoding? encoding = null)
    {
        _content = content ?? throw new ArgumentNullException(nameof(content));
        _encoding = encoding ?? Encoding.UTF8;
    }

    /// <inheritdoc/>
    public PipeReader GetPipeReader(CancellationToken cancellationToken = default)
    {
        return Utilities.CreatePipeReaderFromString(_content, _encoding);
    }
}

