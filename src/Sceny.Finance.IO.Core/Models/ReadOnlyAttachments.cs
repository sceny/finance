using System.Collections;

namespace Sceny.Finance.IO;

/// <summary>
/// Zero-allocation collection of attachments.
/// Provides enumeration and indexing support for attachment collections.
/// </summary>
public readonly struct ReadOnlyAttachments : IEnumerable<Attachment>
{
    private readonly ReadOnlyMemory<Attachment> _attachments;

    /// <summary>
    /// Creates a ReadOnlyAttachments from an array of attachments.
    /// </summary>
    public ReadOnlyAttachments(ReadOnlyMemory<Attachment> attachments)
    {
        _attachments = attachments;
    }

    /// <summary>
    /// Gets the number of attachments.
    /// </summary>
    public int Count => _attachments.Length;

    /// <summary>
    /// Gets whether the Attachments collection is empty.
    /// </summary>
    public bool IsEmpty => _attachments.IsEmpty;

    /// <summary>
    /// Gets the attachment at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the attachment</param>
    /// <returns>The attachment at the specified index</returns>
    public Attachment this[int index] => _attachments.Span[index];

    /// <summary>
    /// Returns an enumerator that iterates through the attachments.
    /// </summary>
    public Enumerator GetEnumerator() => new Enumerator(_attachments);

    IEnumerator<Attachment> IEnumerable<Attachment>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Zero-allocation enumerator for ReadOnlyAttachments.
    /// </summary>
    public struct Enumerator : IEnumerator<Attachment>
    {
        private readonly ReadOnlyMemory<Attachment> _attachments;
        private int _index;

        internal Enumerator(ReadOnlyMemory<Attachment> attachments)
        {
            _attachments = attachments;
            _index = -1;
        }

        public Attachment Current => _attachments.Span[_index];

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            _index++;
            return _index < _attachments.Length;
        }

        public void Reset()
        {
            _index = -1;
        }

        public void Dispose()
        {
        }
    }
}

