using FluentAssertions;

namespace WebApi.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var result = 42;
        result.Should().Be(42);
    }
}