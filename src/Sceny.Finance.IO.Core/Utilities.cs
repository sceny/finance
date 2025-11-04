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
}

