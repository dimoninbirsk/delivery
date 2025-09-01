using Xunit;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using FluentAssertions;
using System;

namespace DeliveryApp.UnitTests.Domain.Model.SharedKernel;

public class LocationShould
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(10, 10)]
    public void BeCorrectWhenParamsIsCorrectOnCreation(int x, int y)
    {
        //Arrange

        //Act
        var location = new Location(x, y);

        //Assert
        location.Should().NotBeNull();
        location.X.Should().Be(x);
        location.Y.Should().Be(y);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(11, 11)]
    [InlineData(11, 5)]
    [InlineData(5, 11)]
    public void ReturnErrorWhenParamsIsIncorrectOnCreation(int x, int y)
    {
        //Arrange

        //Act

        //Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Location(x, y));
    }

    [Fact]
    public void ReturnTrueWhenLocationsAreEqual()
    {
        //Arrange

        //Act
        var firstLocation = new Location(2, 2);
        var secondLocation = new Location(2, 2);

        //Assert
        Assert.Equal(firstLocation, secondLocation);
        Assert.True(firstLocation.Equals(secondLocation));
    }

    [Fact]
    public void ReturnFalseWhenLocationsAreNotEqual()
    {
        //Arrange

        //Act
        var firstLocation = new Location(3, 2);
        var secondLocation = new Location(2, 2);

        //Assert
        Assert.NotEqual(firstLocation, secondLocation);
        Assert.False(firstLocation.Equals(secondLocation));
    }

    [Fact]
    public void ReturnValidLocationWhenReturnRandomLocation()
    {
        //Arrange

        //Act
        var randomLocation = Location.GetRandomLocation();

        //Assert
        randomLocation.Should().NotBeNull();
        randomLocation.X.Should().BeInRange(1, 10);
        randomLocation.Y.Should().BeInRange(1, 10);
    }

    [Theory]
    [InlineData(2, 2, 5, 5, 6)]
    [InlineData(5, 5, 2, 2, 6)]
    [InlineData(2, 2, 2, 2, 0)]
    public void ReturnCorrectDistance(
        int fromX, int fromY,
        int toX, int toY,
        int expectedDistance)
    {
        //Arrange

        //Act
        var distance = Location.GetDistance(new Location(fromX, fromY), new Location(toX, toY));

        //Assert
        distance.Should().Be(expectedDistance);
    }
}