using Sceny.Finance.IO;
using Sceny.Finance.IO.Targets;

namespace Sceny.Finance.IO.Tests;

public class TargetBuilderTests
{
    [Fact]
    public void Target_WithValidTarget_ReturnsTarget()
    {
        // Arrange
        var sb = new System.Text.StringBuilder();
        var target = new StringTarget(sb);
        var builder = new TargetBuilder<StringTarget>(target);

        // Act
        var result = builder.Target;

        // Assert
        Assert.NotNull(result);
        Assert.Same(target, result);
    }

    [Fact]
    public void Constructor_WithNullTarget_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TargetBuilder<StringTarget>(null!));
    }
}

