using Domain;
using Domain.Datapoint;
using Domain.Exceptions;
using FluentAssertions;

namespace WebApi.Tests.UnitTests.Domain;

public class DataPointTests
{
    [Fact]
    public void InstantiateDataPoint_FailsWhenDivideByZero()
    {
        // Act
        var act = () => new DataPoint(IdGenerator.GenerateId(), "Key")
        {
            Formula = new Formula(MathOperation.Divide)
        };

        //Assert
        act.Should().Throw<CannotDivideWithZeroException>();
    }

    [Theory]
    [InlineData(MathOperation.Add, 1.5, 10, 11.5)]
    [InlineData(MathOperation.Subtract, 15, 10, -5)]
    [InlineData(MathOperation.Multiply, 3, 7, 21)]
    [InlineData(MathOperation.Divide, 4, 25, 6.25)]
    [InlineData(MathOperation.None, 1.5, 10, 10)]
    public void SetLatestValueBasedOnFormula_CalculatesCorrectLatestValue(MathOperation operation, double factor,
        double value, double expectedResult)
    {
        //Arrange
        var dataPoint = new DataPoint(IdGenerator.GenerateId(), "Key")
        {
            Formula = new Formula(operation, factor)
        };

        //Act
        dataPoint.SetLatestValueBasedOnFormula(value);

        //Assert
        dataPoint.LatestValue.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData( true, 30, 22.5, 7.5)]
    [InlineData( false, 30, 22.5, 33.33)]
    [InlineData( false, 15, 30, -50)]
    [InlineData( true, 15, 30, -15)]
    [InlineData( true, 30, 0, 30)]
    [InlineData( true, 0, 30, -30)]
    public void CalculateChangeOverTime_ReturnsCorrectResult(bool comparisonIsAbsolute, double latestValue, double oldValue,
        double expectedResult)
    {
        // Arrange
        var dataPoint = new DataPoint(IdGenerator.GenerateId(), "Key")
        {
            LatestValue = latestValue,
            ComparisonIsAbsolute = comparisonIsAbsolute
        };
        
        // Act
        var result = dataPoint.CalculateChangeOverTime(oldValue);
        
        // Assert
        Math.Round(result, 2).Should().Be(expectedResult);
    }
}