using Sceny.Finance.IO;

namespace Sceny.Finance.IO.Tests;

public class AttachmentTests
{
    [Fact]
    public void Constructor_WithAllParameters_CreatesAttachment()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var name = "test.pdf";
        var contentType = "application/pdf";
        var reference = "ref://test";

        // Act
        var attachment = new Attachment(name, contentType, new ReadOnlyMemory<byte>(data), reference);

        // Assert
        Assert.Equal(name, attachment.Name.ToString());
        Assert.Equal(contentType, attachment.ContentType.ToString());
        Assert.Equal(data, attachment.Data.ToArray());
        Assert.Equal(reference, attachment.Reference.ToString());
    }

    [Fact]
    public void Constructor_WithoutReference_CreatesAttachmentWithDefaultReference()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3 };
        var name = "test.pdf";
        var contentType = "application/pdf";

        // Act
        var attachment = new Attachment(name, contentType, new ReadOnlyMemory<byte>(data));

        // Assert
        Assert.Equal(name, attachment.Name.ToString());
        Assert.Equal(contentType, attachment.ContentType.ToString());
        Assert.Equal(data, attachment.Data.ToArray());
        Assert.True(attachment.Reference.IsEmpty);
    }

    [Fact]
    public void HasData_WithData_ReturnsTrue()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3 };
        var attachment = new Attachment("test.pdf", "application/pdf", new ReadOnlyMemory<byte>(data));

        // Act
        var hasData = attachment.HasData;

        // Assert
        Assert.True(hasData);
    }

    [Fact]
    public void HasData_WithEmptyData_ReturnsFalse()
    {
        // Arrange
        var attachment = new Attachment("test.pdf", "application/pdf", new ReadOnlyMemory<byte>(new byte[0]));

        // Act
        var hasData = attachment.HasData;

        // Assert
        Assert.False(hasData);
    }

    [Fact]
    public void Size_WithData_ReturnsDataLength()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var attachment = new Attachment("test.pdf", "application/pdf", new ReadOnlyMemory<byte>(data));

        // Act
        var size = attachment.Size;

        // Assert
        Assert.Equal(5, size);
    }

    [Fact]
    public void Size_WithEmptyData_ReturnsZero()
    {
        // Arrange
        var attachment = new Attachment("test.pdf", "application/pdf", new ReadOnlyMemory<byte>(new byte[0]));

        // Act
        var size = attachment.Size;

        // Assert
        Assert.Equal(0, size);
    }

    [Fact]
    public void Constructor_WithEmptyName_AllowsEmptyName()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3 };

        // Act
        var attachment = new Attachment("", "application/pdf", new ReadOnlyMemory<byte>(data));

        // Assert
        Assert.True(attachment.Name.IsEmpty);
    }

    [Fact]
    public void Constructor_WithEmptyContentType_AllowsEmptyContentType()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3 };

        // Act
        var attachment = new Attachment("test.pdf", "", new ReadOnlyMemory<byte>(data));

        // Assert
        Assert.True(attachment.ContentType.IsEmpty);
    }
}




















