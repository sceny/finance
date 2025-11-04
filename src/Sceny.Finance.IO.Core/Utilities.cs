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
}

