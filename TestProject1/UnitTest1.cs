using ClassLibrary1;
using FluentAssertions;

namespace TestProject1;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        Class1.Test().Should().NotBeEmpty();
    }
}