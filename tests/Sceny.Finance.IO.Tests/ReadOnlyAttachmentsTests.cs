using System.Collections.Generic;
using System.Linq;
using Sceny.Finance.IO;

namespace Sceny.Finance.IO.Tests;

public class ReadOnlyAttachmentsTests
{
    [Fact]
    public void Constructor_WithAttachments_CreatesReadOnlyAttachments()
    {
        // Arrange
        var attachments = new Attachment[]
        {
            new Attachment("file1.pdf", "application/pdf", new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3 })),
            new Attachment("file2.png", "image/png", new ReadOnlyMemory<byte>(new byte[] { 4, 5, 6 }))
        };

        // Act
        var readonlyAttachments = new ReadOnlyAttachments(new ReadOnlyMemory<Attachment>(attachments));

        // Assert
        Assert.Equal(2, readonlyAttachments.Count);
        Assert.False(readonlyAttachments.IsEmpty);
    }

    [Fact]
    public void Constructor_WithEmptyAttachments_CreatesEmptyReadOnlyAttachments()
    {
        // Arrange
        var attachments = new Attachment[0];

        // Act
        var readonlyAttachments = new ReadOnlyAttachments(new ReadOnlyMemory<Attachment>(attachments));

        // Assert
        Assert.Equal(0, readonlyAttachments.Count);
        Assert.True(readonlyAttachments.IsEmpty);
    }

    [Fact]
    public void Indexer_WithValidIndex_ReturnsAttachment()
    {
        // Arrange
        var attachments = new Attachment[]
        {
            new Attachment("file1.pdf", "application/pdf", new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3 })),
            new Attachment("file2.png", "image/png", new ReadOnlyMemory<byte>(new byte[] { 4, 5, 6 }))
        };
        var readonlyAttachments = new ReadOnlyAttachments(new ReadOnlyMemory<Attachment>(attachments));

        // Act
        var attachment = readonlyAttachments[0];

        // Assert
        Assert.Equal("file1.pdf", attachment.Name.ToString());
        Assert.Equal("application/pdf", attachment.ContentType.ToString());
    }

    [Fact]
    public void Indexer_WithLastIndex_ReturnsLastAttachment()
    {
        // Arrange
        var attachments = new Attachment[]
        {
            new Attachment("file1.pdf", "application/pdf", new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3 })),
            new Attachment("file2.png", "image/png", new ReadOnlyMemory<byte>(new byte[] { 4, 5, 6 }))
        };
        var readonlyAttachments = new ReadOnlyAttachments(new ReadOnlyMemory<Attachment>(attachments));

        // Act
        var attachment = readonlyAttachments[1];

        // Assert
        Assert.Equal("file2.png", attachment.Name.ToString());
        Assert.Equal("image/png", attachment.ContentType.ToString());
    }

    [Fact]
    public void GetEnumerator_EnumeratesAllAttachments()
    {
        // Arrange
        var attachments = new Attachment[]
        {
            new Attachment("file1.pdf", "application/pdf", new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3 })),
            new Attachment("file2.png", "image/png", new ReadOnlyMemory<byte>(new byte[] { 4, 5, 6 })),
            new Attachment("file3.txt", "text/plain", new ReadOnlyMemory<byte>(new byte[] { 7, 8, 9 }))
        };
        var readonlyAttachments = new ReadOnlyAttachments(new ReadOnlyMemory<Attachment>(attachments));

        // Act
        var enumerated = readonlyAttachments.ToList();

        // Assert
        Assert.Equal(3, enumerated.Count);
        Assert.Equal("file1.pdf", enumerated[0].Name.ToString());
        Assert.Equal("file2.png", enumerated[1].Name.ToString());
        Assert.Equal("file3.txt", enumerated[2].Name.ToString());
    }

    [Fact]
    public void GetEnumerator_WithEmptyAttachments_EnumeratesNothing()
    {
        // Arrange
        var readonlyAttachments = new ReadOnlyAttachments(new ReadOnlyMemory<Attachment>(new Attachment[0]));

        // Act
        var enumerated = readonlyAttachments.ToList();

        // Assert
        Assert.Empty(enumerated);
    }

    [Fact]
    public void Enumerator_Reset_ResetsToBeginning()
    {
        // Arrange
        var attachments = new Attachment[]
        {
            new Attachment("file1.pdf", "application/pdf", new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3 })),
            new Attachment("file2.png", "image/png", new ReadOnlyMemory<byte>(new byte[] { 4, 5, 6 }))
        };
        var readonlyAttachments = new ReadOnlyAttachments(new ReadOnlyMemory<Attachment>(attachments));
        var enumerator = readonlyAttachments.GetEnumerator();

        // Act
        enumerator.MoveNext();
        var first = enumerator.Current;
        enumerator.Reset();
        enumerator.MoveNext();
        var afterReset = enumerator.Current;

        // Assert
        Assert.Equal(first.Name, afterReset.Name);
        Assert.Equal(first.ContentType, afterReset.ContentType);
    }

    [Fact]
    public void Enumerator_Dispose_DoesNotThrow()
    {
        // Arrange
        var attachments = new Attachment[]
        {
            new Attachment("file1.pdf", "application/pdf", new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3 }))
        };
        var readonlyAttachments = new ReadOnlyAttachments(new ReadOnlyMemory<Attachment>(attachments));
        var enumerator = readonlyAttachments.GetEnumerator();

        // Act & Assert
        enumerator.Dispose(); // Should not throw
    }

    [Fact]
    public void IEnumerable_GetEnumerator_ReturnsEnumerator()
    {
        // Arrange
        var attachments = new Attachment[]
        {
            new Attachment("file1.pdf", "application/pdf", new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3 }))
        };
        var readonlyAttachments = new ReadOnlyAttachments(new ReadOnlyMemory<Attachment>(attachments));

        // Act
        var enumerator = ((IEnumerable<Attachment>)readonlyAttachments).GetEnumerator();

        // Assert
        Assert.NotNull(enumerator);
        Assert.True(enumerator.MoveNext());
        Assert.Equal("file1.pdf", enumerator.Current.Name.ToString());
    }

    [Fact]
    public void IEnumerable_NonGeneric_GetEnumerator_ReturnsEnumerator()
    {
        // Arrange
        var attachments = new Attachment[]
        {
            new Attachment("file1.pdf", "application/pdf", new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3 }))
        };
        var readonlyAttachments = new ReadOnlyAttachments(new ReadOnlyMemory<Attachment>(attachments));

        // Act
        var enumerator = ((System.Collections.IEnumerable)readonlyAttachments).GetEnumerator();

        // Assert
        Assert.NotNull(enumerator);
        Assert.True(enumerator.MoveNext());
        var attachment = (Attachment)enumerator.Current;
        Assert.Equal("file1.pdf", attachment.Name.ToString());
    }
}

