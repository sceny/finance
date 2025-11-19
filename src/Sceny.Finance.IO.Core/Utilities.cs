using System.Buffers;
using System.IO.Pipelines;
using Microsoft.Extensions.ObjectPool;

namespace Sceny.Finance.IO;

/// <summary>
/// Utility methods for pipeline operations and object pooling.
/// </summary>
public static class Utilities
{
    private static readonly ArrayPool<byte> BytePool = ArrayPool<byte>.Shared;
    private static readonly ArrayPool<char> CharPool = ArrayPool<char>.Shared;

    /// <summary>
    /// Gets a shared byte array pool for temporary buffers.
    /// </summary>
    public static ArrayPool<byte> ByteArrayPool => BytePool;

    /// <summary>
    /// Gets a shared char array pool for temporary buffers.
    /// </summary>
    public static ArrayPool<char> CharArrayPool => CharPool;

    /// <summary>
    /// Reads a complete sequence from the PipeReader.
    /// Returns a ReadOnlySequence that can be processed without allocations.
    /// </summary>
    public static async Task<ReadOnlySequence<byte>> ReadCompleteSequenceAsync(
        PipeReader reader,
        CancellationToken cancellationToken = default)
    {
        ReadResult result;
        ReadOnlySequence<byte> buffer = default;

        while (true)
        {
            result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            buffer = result.Buffer;

            if (result.IsCompleted || result.IsCanceled)
                break;

            if (buffer.Length > 0)
                break;

            reader.AdvanceTo(buffer.Start, buffer.End);
        }

        return buffer;
    }

    /// <summary>
    /// Advances the PipeReader to the end of the consumed data.
    /// </summary>
    public static void AdvanceReader(PipeReader reader, ReadOnlySequence<byte> consumed)
    {
        reader.AdvanceTo(consumed.End);
    }

    /// <summary>
    /// Converts a ReadOnlySequence of bytes to a string using the specified encoding.
    /// Uses pooled buffers when possible.
    /// </summary>
    public static string SequenceToString(ReadOnlySequence<byte> sequence, System.Text.Encoding encoding)
    {
        if (sequence.Length == 0)
        {
            return string.Empty;
        }
        
        if (sequence.IsSingleSegment)
        {
            return encoding.GetString(sequence.FirstSpan);
        }

        // Multi-segment sequence - copy to byte array first
        var byteLength = (int)sequence.Length;
        var byteBuffer = BytePool.Rent(byteLength);

        try
        {
            sequence.CopyTo(byteBuffer);
            var span = new ReadOnlySpan<byte>(byteBuffer, 0, byteLength);
            
            // Get char count first
            var charCount = encoding.GetCharCount(span);
            var charBuffer = CharArrayPool.Rent(charCount);

            try
            {
                encoding.GetChars(span, charBuffer.AsSpan());
                return new string(charBuffer, 0, charCount);
            }
            finally
            {
                CharArrayPool.Return(charBuffer);
            }
        }
        finally
        {
            BytePool.Return(byteBuffer);
        }
    }

    /// <summary>
    /// Gets a ReadOnlyMemory from a string for use in zero-allocation scenarios.
    /// The returned memory references the string's underlying memory.
    /// </summary>
    public static ReadOnlyMemory<char> StringToMemory(string value)
    {
        return value.AsMemory();
    }

    /// <summary>
    /// Creates a PipeReader from a string with zero-allocation pattern.
    /// Uses GetSpan/Advance pattern for optimal performance.
    /// </summary>
    /// <param name="content">The string content to convert</param>
    /// <param name="encoding">The encoding to use (default: UTF-8)</param>
    /// <returns>A PipeReader containing the encoded string content</returns>
    public static PipeReader CreatePipeReaderFromString(string content, System.Text.Encoding? encoding = null)
    {
        encoding ??= System.Text.Encoding.UTF8;
        var pipe = new Pipe();
        var writer = pipe.Writer;
        var bytes = encoding.GetBytes(content);
        var span = writer.GetSpan(bytes.Length);
        bytes.CopyTo(span);
        writer.Advance(bytes.Length);
        writer.Complete();
        return pipe.Reader;
    }

    /// <summary>
    /// Creates a PipeReader from a byte array with zero-allocation pattern.
    /// Uses GetSpan/Advance pattern for optimal performance.
    /// </summary>
    /// <param name="data">The byte data to convert</param>
    /// <returns>A PipeReader containing the byte data</returns>
    public static PipeReader CreatePipeReaderFromBytes(ReadOnlyMemory<byte> data)
    {
        var pipe = new Pipe();
        var writer = pipe.Writer;
        var span = writer.GetSpan(data.Length);
        data.Span.CopyTo(span);
        writer.Advance(data.Length);
        writer.Complete();
        return pipe.Reader;
    }

    /// <summary>
    /// Creates a PipeReader from a Stream for production use.
    /// The stream is copied to the pipe asynchronously.
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A PipeReader containing the stream data</returns>
    public static async Task<PipeReader> CreatePipeReaderFromStreamAsync(
        System.IO.Stream stream,
        CancellationToken cancellationToken = default)
    {
        var pipe = new Pipe();
        var writer = pipe.Writer;

        try
        {
            var buffer = new byte[8192];
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                var span = writer.GetSpan(bytesRead);
                buffer.AsSpan(0, bytesRead).CopyTo(span);
                writer.Advance(bytesRead);
            }

            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await writer.CompleteAsync().ConfigureAwait(false);
        }

        return pipe.Reader;
    }

    /// <summary>
    /// Creates a PipeReader from a file path for production use.
    /// </summary>
    /// <param name="filePath">The path to the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A PipeReader containing the file data</returns>
    public static async Task<PipeReader> CreatePipeReaderFromFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var stream = new System.IO.FileStream(
            filePath,
            System.IO.FileMode.Open,
            System.IO.FileAccess.Read,
            System.IO.FileShare.Read,
            bufferSize: 8192,
            useAsync: true);

        try
        {
            var reader = await CreatePipeReaderFromStreamAsync(stream, cancellationToken).ConfigureAwait(false);
            // Dispose the stream after creating the reader since all data has been copied
            await stream.DisposeAsync().ConfigureAwait(false);
            return reader;
        }
        catch
        {
            await stream.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Creates a PipeWriter from a Stream for production use.
    /// The pipe writer writes directly to the stream using zero-allocation pattern.
    /// </summary>
    /// <param name="stream">The stream to write to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A PipeWriter that writes to the stream</returns>
    public static async Task<PipeWriter> CreatePipeWriterFromStreamAsync(
        System.IO.Stream stream,
        CancellationToken cancellationToken = default)
    {
        var pipe = new Pipe();
        var reader = pipe.Reader;

        // Start background task to copy from pipe to stream
        _ = Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    var result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                    var buffer = result.Buffer;

                    if (buffer.Length > 0)
                    {
                        foreach (var segment in buffer)
                        {
                            await stream.WriteAsync(segment, cancellationToken).ConfigureAwait(false);
                        }
                        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                    }

                    reader.AdvanceTo(buffer.End);

                    if (result.IsCompleted || result.IsCanceled)
                        break;
                }
            }
            finally
            {
                await reader.CompleteAsync().ConfigureAwait(false);
            }
        }, cancellationToken);

        return pipe.Writer;
    }

    /// <summary>
    /// Creates a PipeWriter from a file path for production use.
    /// </summary>
    /// <param name="filePath">The path to the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A PipeWriter that writes to the file</returns>
    public static async Task<PipeWriter> CreatePipeWriterFromFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var stream = new System.IO.FileStream(
            filePath,
            System.IO.FileMode.Create,
            System.IO.FileAccess.Write,
            System.IO.FileShare.None,
            bufferSize: 8192,
            useAsync: true);

        var pipe = new Pipe();
        var reader = pipe.Reader;

        // Start background task to copy from pipe to stream and dispose stream when done
        _ = Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    var result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                    var buffer = result.Buffer;

                    if (buffer.Length > 0)
                    {
                        foreach (var segment in buffer)
                        {
                            await stream.WriteAsync(segment, cancellationToken).ConfigureAwait(false);
                        }
                        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                    }

                    reader.AdvanceTo(buffer.End);

                    if (result.IsCompleted || result.IsCanceled)
                        break;
                }
            }
            finally
            {
                await reader.CompleteAsync().ConfigureAwait(false);
                await stream.DisposeAsync().ConfigureAwait(false);
            }
        }, cancellationToken);

        return pipe.Writer;
    }

    /// <summary>
    /// Creates a PipeWriter from an IBufferWriter with zero-allocation pattern.
    /// Uses GetSpan/Advance pattern for optimal performance.
    /// </summary>
    /// <param name="bufferWriter">The buffer writer to write to</param>
    /// <returns>A PipeWriter that writes to the buffer writer</returns>
    public static PipeWriter CreatePipeWriterFromBufferWriter(IBufferWriter<byte> bufferWriter)
    {
        var pipe = new Pipe();
        var reader = pipe.Reader;

        // Start background task to copy from pipe to buffer writer
        _ = Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    var result = await reader.ReadAsync().ConfigureAwait(false);
                    var buffer = result.Buffer;

                    if (buffer.Length > 0)
                    {
                        foreach (var segment in buffer)
                        {
                            var span = bufferWriter.GetSpan(segment.Length);
                            segment.Span.CopyTo(span);
                            bufferWriter.Advance(segment.Length);
                        }
                    }

                    reader.AdvanceTo(buffer.End);

                    if (result.IsCompleted || result.IsCanceled)
                        break;
                }
            }
            finally
            {
                await reader.CompleteAsync().ConfigureAwait(false);
            }
        });

        return pipe.Writer;
    }

    /// <summary>
    /// Creates a PipeWriter from a StringBuilder with zero-allocation pattern.
    /// Uses GetSpan/Advance pattern for optimal performance.
    /// </summary>
    /// <param name="stringBuilder">The string builder to write to</param>
    /// <param name="encoding">The encoding to use (default: UTF-8)</param>
    /// <returns>A PipeWriter that writes to the string builder</returns>
    public static PipeWriter CreatePipeWriterFromStringBuilder(System.Text.StringBuilder stringBuilder, System.Text.Encoding? encoding = null)
    {
        encoding ??= System.Text.Encoding.UTF8;
        var pipe = new Pipe();
        var writer = pipe.Writer;
        var reader = pipe.Reader;

        // Start background task to decode and write to StringBuilder
        _ = Task.Run(async () =>
        {
            try
            {
                var decoder = encoding.GetDecoder();
                var charBuffer = CharPool.Rent(4096);

                try
                {
                    while (true)
                    {
                        var result = await reader.ReadAsync().ConfigureAwait(false);
                        var buffer = result.Buffer;

                        if (buffer.Length > 0)
                        {
                            // Process each segment of the buffer
                            foreach (var segment in buffer)
                            {
                                var charCount = decoder.GetCharCount(segment.Span, flush: false);
                                if (charCount > charBuffer.Length)
                                {
                                    CharPool.Return(charBuffer);
                                    charBuffer = CharPool.Rent(charCount);
                                }

                                var charsWritten = decoder.GetChars(segment.Span, charBuffer.AsSpan(0, charCount), flush: false);
                                stringBuilder.Append(charBuffer, 0, charsWritten);
                            }
                        }

                        reader.AdvanceTo(buffer.End);

                        if (result.IsCompleted || result.IsCanceled)
                        {
                            // Flush remaining characters
                            var finalCharCount = decoder.GetCharCount(ReadOnlySpan<byte>.Empty, flush: true);
                            if (finalCharCount > 0)
                            {
                                if (finalCharCount > charBuffer.Length)
                                {
                                    CharPool.Return(charBuffer);
                                    charBuffer = CharPool.Rent(finalCharCount);
                                }
                                var finalCharsWritten = decoder.GetChars(ReadOnlySpan<byte>.Empty, charBuffer.AsSpan(0, finalCharCount), flush: true);
                                stringBuilder.Append(charBuffer, 0, finalCharsWritten);
                            }
                            break;
                        }
                    }
                }
                finally
                {
                    CharPool.Return(charBuffer);
                    await reader.CompleteAsync().ConfigureAwait(false);
                }
            }
            catch
            {
                await reader.CompleteAsync().ConfigureAwait(false);
                throw;
            }
        });

        return writer;
    }

    /// <summary>
    /// Copies data from a PipeReader to a PipeWriter in a zero-allocation way.
    /// Uses GetSpan/Advance pattern for optimal performance.
    /// </summary>
    /// <param name="reader">The pipe reader to read from</param>
    /// <param name="writer">The pipe writer to write to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the copy operation</returns>
    public static async Task CopyPipeAsync(
        PipeReader reader,
        PipeWriter writer,
        CancellationToken cancellationToken = default)
    {
        try
        {
            while (true)
            {
                var result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                var buffer = result.Buffer;

                if (buffer.Length > 0)
                {
                    foreach (var segment in buffer)
                    {
                        var span = writer.GetSpan(segment.Length);
                        segment.Span.CopyTo(span);
                        writer.Advance(segment.Length);
                    }

                    await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                }

                reader.AdvanceTo(buffer.End);

                if (result.IsCompleted || result.IsCanceled)
                    break;
            }
        }
        finally
        {
            await writer.CompleteAsync().ConfigureAwait(false);
            await reader.CompleteAsync().ConfigureAwait(false);
        }
    }
}

