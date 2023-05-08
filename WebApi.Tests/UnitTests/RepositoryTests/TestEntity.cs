using Repository;

namespace WebApi.Tests.UnitTests.RepositoryTests;

[BsonCollection("test-document")]
public class TestEntity : Document
{
    public TestEntity(string stringValue, int intValue, double doubleValue)
    {
        StringValue = stringValue;
        IntValue = intValue;
        DoubleValue = doubleValue;
    }

    public string StringValue { get; set; }
    public int IntValue { get; set; }
    public double DoubleValue { get; set; }
}