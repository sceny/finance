using System.IO;
using Sceny.Finance.IO.Targets;

namespace Sceny.Finance.IO;

/// <summary>
/// Main entry point for writing financial data to various targets.
/// Provides a fluent API for configuring targets and format writers.
/// </summary>
public static class FinanceWriter
{
    /// <summary>
    /// Creates a target builder for writing to a file.
    /// </summary>
    /// <param name="filePath">The path to the file</param>
    /// <returns>A target builder for the file</returns>
    public static TargetBuilder<FileTarget> ToFile(string filePath)
    {
        return new(new(filePath));
    }

    /// <summary>
    /// Creates a target builder for writing to a stream.
    /// </summary>
    /// <param name="stream">The stream to write to</param>
    /// <param name="disposeStream">Whether to dispose the stream when done (default: false)</param>
    /// <returns>A target builder for the stream</returns>
    public static TargetBuilder<StreamTarget> ToStream(Stream stream, bool disposeStream = false)
    {
        return new(new(stream, disposeStream));
    }

    /// <summary>
    /// Creates a target builder for writing to in-memory byte data.
    /// </summary>
    /// <param name="data">The byte data buffer to write to</param>
    /// <returns>A target builder for the memory data</returns>
    public static TargetBuilder<MemoryTarget> ToBytes(System.Buffers.IBufferWriter<byte> data)
    {
        return new(new(data));
    }

    /// <summary>
    /// Creates a target builder for writing to a string builder.
    /// </summary>
    /// <param name="stringBuilder">The string builder to write to</param>
    /// <param name="encoding">The encoding to use (default: UTF-8)</param>
    /// <returns>A target builder for the string builder</returns>
    public static TargetBuilder<StringTarget> ToString(System.Text.StringBuilder stringBuilder, System.Text.Encoding? encoding = null)
    {
        return new(new(stringBuilder, encoding));
    }
}

