using System.IO;
using Sceny.Finance.IO.Sources;

namespace Sceny.Finance.IO;

/// <summary>
/// Main entry point for reading financial data from various sources.
/// Provides a fluent API for configuring sources and format parsers.
/// </summary>
public static class FinanceReader
{
    /// <summary>
    /// Creates a source builder for reading from a file.
    /// </summary>
    /// <param name="filePath">The path to the file</param>
    /// <returns>A source builder for the file</returns>
    public static SourceBuilder<FileSource> FromFile(string filePath)
    {
        return new(new(filePath));
    }

    /// <summary>
    /// Creates a source builder for reading from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <param name="disposeStream">Whether to dispose the stream when done (default: false)</param>
    /// <returns>A source builder for the stream</returns>
    public static SourceBuilder<StreamSource> FromStream(Stream stream, bool disposeStream = false)
    {
        return new(new(stream, disposeStream));
    }

    /// <summary>
    /// Creates a source builder for reading from in-memory byte data.
    /// </summary>
    /// <param name="data">The byte data to read from</param>
    /// <returns>A source builder for the memory data</returns>
    public static SourceBuilder<MemorySource> FromBytes(ReadOnlyMemory<byte> data)
    {
        return new(new(data));
    }

    /// <summary>
    /// Creates a source builder for reading from a string.
    /// </summary>
    /// <param name="content">The string content to read from</param>
    /// <param name="encoding">The encoding to use (default: UTF-8)</param>
    /// <returns>A source builder for the string</returns>
    public static SourceBuilder<StringSource> FromString(string content, System.Text.Encoding? encoding = null)
    {
        return new(new(content, encoding));
    }
}

