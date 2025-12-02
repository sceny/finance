using Sceny.Finance.IO;

namespace Sceny.Finance.IO.Tests;

public class ArgumentExceptionExtensionsTests
{
    // String overload tests

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WithNullString_ThrowsArgumentNullException()
    {
        // Arrange
        string? argument = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            ArgumentException.ThrowIfNullOrWhiteSpace(argument));
        
        Assert.Equal(nameof(argument), exception.ParamName);
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WithEmptyString_ThrowsArgumentException()
    {
        // Arrange
        var argument = string.Empty;

        // Act & Assert
        // Empty string is caught by ThrowIfNullOrEmpty first, which throws with its own message
        var exception = Assert.Throws<ArgumentException>(() => 
            ArgumentException.ThrowIfNullOrWhiteSpace(argument));
        
        Assert.Equal(nameof(argument), exception.ParamName);
        // ThrowIfNullOrEmpty throws with its own message, not the whitespace message
        Assert.Contains("empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WithWhitespaceOnly_ThrowsArgumentException()
    {
        // Arrange
        var argument = "   ";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ArgumentException.ThrowIfNullOrWhiteSpace(argument));
        
        Assert.Equal(nameof(argument), exception.ParamName);
        // Check that the message contains "whitespace" to verify it's using our custom message
        Assert.Contains("whitespace", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WithWhitespaceIncludingNewline_ThrowsArgumentException()
    {
        // Arrange
        var argument = " \t\n\r ";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ArgumentException.ThrowIfNullOrWhiteSpace(argument));
        
        Assert.Equal(nameof(argument), exception.ParamName);
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WithValidString_DoesNotThrow()
    {
        // Arrange
        var argument = "valid";

        // Act & Assert
        ArgumentException.ThrowIfNullOrWhiteSpace(argument);
        // Should not throw
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WithStringContainingWhitespace_DoesNotThrow()
    {
        // Arrange
        var argument = "valid string";

        // Act & Assert
        ArgumentException.ThrowIfNullOrWhiteSpace(argument);
        // Should not throw - whitespace is allowed if there are non-whitespace characters
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WithCustomMessage_UsesCustomMessage()
    {
        // Arrange
        var argument = "   ";
        var customMessage = "Custom error message";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ArgumentException.ThrowIfNullOrWhiteSpace(argument, whiteSpaceMessage: customMessage));
        
        Assert.Equal(nameof(argument), exception.ParamName);
        // ArgumentException.Message includes the parameter name, so check Contains
        Assert.Contains(customMessage, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WithCustomParamName_UsesCustomParamName()
    {
        // Arrange
        var argument = "   ";
        var customParamName = "customParam";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ArgumentException.ThrowIfNullOrWhiteSpace(argument, paramName: customParamName));
        
        Assert.Equal(customParamName, exception.ParamName);
    }

    // ReadOnlySpan<char> overload tests

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WithEmptySpan_ThrowsArgumentException()
    {
        // Act & Assert
        // CallerArgumentExpression captures the expression, not the parameter name
        var exception = Assert.Throws<ArgumentException>(() => 
            ArgumentException.ThrowIfNullOrWhiteSpace(ReadOnlySpan<char>.Empty));
        
        // CallerArgumentExpression will capture "ReadOnlySpan<char>.Empty"
        Assert.NotNull(exception.ParamName);
        Assert.Contains("Value cannot be empty or whitespace.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WithWhitespaceOnlySpan_ThrowsArgumentException()
    {
        // Arrange
        var text = "   ";

        // Act & Assert
        // CallerArgumentExpression captures the expression "text.AsSpan()"
        var exception = Assert.Throws<ArgumentException>(() => 
            ArgumentException.ThrowIfNullOrWhiteSpace(text.AsSpan()));
        
        // CallerArgumentExpression captures the expression, not the parameter name
        Assert.NotNull(exception.ParamName);
        Assert.Contains("Value cannot be empty or whitespace.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WithWhitespaceIncludingNewlineSpan_ThrowsArgumentException()
    {
        // Arrange
        var text = " \t\n\r ";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ArgumentException.ThrowIfNullOrWhiteSpace(text.AsSpan()));
        
        Assert.NotNull(exception.ParamName);
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WithValidSpan_DoesNotThrow()
    {
        // Arrange
        var text = "valid";

        // Act & Assert
        ArgumentException.ThrowIfNullOrWhiteSpace(text.AsSpan());
        // Should not throw
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WithSpanContainingWhitespace_DoesNotThrow()
    {
        // Arrange
        var text = "valid string";

        // Act & Assert
        ArgumentException.ThrowIfNullOrWhiteSpace(text.AsSpan());
        // Should not throw - whitespace is allowed if there are non-whitespace characters
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WithSpanSingleWhitespaceChar_ThrowsArgumentException()
    {
        // Arrange
        var text = " ";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ArgumentException.ThrowIfNullOrWhiteSpace(text.AsSpan()));
        
        Assert.NotNull(exception.ParamName);
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WithSpanCustomMessage_UsesCustomMessage()
    {
        // Arrange
        var text = "   ";
        var customMessage = "Custom error message for span";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ArgumentException.ThrowIfNullOrWhiteSpace(text.AsSpan(), whiteSpaceMessage: customMessage));
        
        Assert.NotNull(exception.ParamName);
        Assert.Contains(customMessage, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WithSpanCustomParamName_UsesCustomParamName()
    {
        // Arrange
        var text = "   ";
        var customParamName = "customSpanParam";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ArgumentException.ThrowIfNullOrWhiteSpace(text.AsSpan(), paramName: customParamName));
        
        Assert.Equal(customParamName, exception.ParamName);
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WithSpanTabOnly_ThrowsArgumentException()
    {
        // Arrange
        var text = "\t";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ArgumentException.ThrowIfNullOrWhiteSpace(text.AsSpan()));
        
        Assert.NotNull(exception.ParamName);
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WithSpanNewlineOnly_ThrowsArgumentException()
    {
        // Arrange
        var text = "\n";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ArgumentException.ThrowIfNullOrWhiteSpace(text.AsSpan()));
        
        Assert.NotNull(exception.ParamName);
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WithSpanCarriageReturnOnly_ThrowsArgumentException()
    {
        // Arrange
        var text = "\r";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ArgumentException.ThrowIfNullOrWhiteSpace(text.AsSpan()));
        
        Assert.NotNull(exception.ParamName);
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WithSpanLeadingWhitespace_DoesNotThrow()
    {
        // Arrange
        var text = "  valid";

        // Act & Assert
        ArgumentException.ThrowIfNullOrWhiteSpace(text.AsSpan());
        // Should not throw - has non-whitespace characters
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WithSpanTrailingWhitespace_DoesNotThrow()
    {
        // Arrange
        var text = "valid  ";

        // Act & Assert
        ArgumentException.ThrowIfNullOrWhiteSpace(text.AsSpan());
        // Should not throw - has non-whitespace characters
    }
}

