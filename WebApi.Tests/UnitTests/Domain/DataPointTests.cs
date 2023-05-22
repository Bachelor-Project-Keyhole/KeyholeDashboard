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
    [InlineData( 30, 22.5, -7.5)]
    [InlineData( 30, 40, 10)]
    [InlineData( 15, -30, -45)]
    [InlineData(-0.5, 1.5, 2)]
    [InlineData(-30, 0, 30)]
    [InlineData( -25, -23, 2)]
    [InlineData( -10, -11.7, -1.7)]
    [InlineData(-5, 0, 5)]
    [InlineData(-5, -5, 0)]
    public void CalculateChangeOverTime_Absolute_ReturnsCorrectResult(double oldValue,
        double newValue,
        double expectedResult)
    {
        // Arrange
        var dataPoint = new DataPoint(IdGenerator.GenerateId(), "Key")
        {
            LatestValue = newValue,
            ComparisonIsAbsolute = true
        };

        // Act
        var result = dataPoint.CalculateChangeOverTime(oldValue);

        // Assert
        Math.Round(result, 2).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(-400, -300, 25)]
    [InlineData(-400, -500, -25)]
    [InlineData(100, 300, 200)]
    [InlineData(200, 100, -50)]
    [InlineData(-200, 100, 150)]
    [InlineData(100, -300, -400)]
    [InlineData(-50, 0, 100)]
    [InlineData(100, 0, -100)]
    [InlineData(0, 50, 100)]
    [InlineData(0, -50, -100)]
    [InlineData(100, 100, 0)]
    [InlineData(0, 0, 0)]
    public void CalculateChangeOverTime_Percentage_ReturnsCorrectResult(double oldValue, double newValue,
        double expectedResult)
    {
        // Arrange
        var dataPoint = new DataPoint(IdGenerator.GenerateId(), "Key")
        {
            LatestValue = newValue,
            ComparisonIsAbsolute = false
        };

        // Act
        var result = dataPoint.CalculateChangeOverTime(oldValue);

        // Assert
        Math.Round(result, 2).Should().Be(expectedResult);
    }
}