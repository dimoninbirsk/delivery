using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using FluentAssertions;
using Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.OrderAggregate;

public class OrderShould
{
    public static IEnumerable<object[]> GetIncorrectOrderParams()
    {
        yield return [Guid.Empty, Location.Create(1,1).Value, 10];
        yield return [Guid.NewGuid(), null, 10];
        yield return [Guid.Empty, Location.Create(1,1).Value, -1];
    }

    [Fact]
    public void DerivedAggregate()
    {
        //Arrange

        //Act
        var isDerivedAggregate = typeof(Order).IsSubclassOf(typeof(Aggregate<Guid>));

        //Assert
        isDerivedAggregate.Should().BeTrue();
    }

    [Fact]
    public void ConstructorShouldBePrivate()
    {
        // Arrange
        var typeInfo = typeof(Order).GetTypeInfo();

        // Act

        // Assert
        typeInfo.DeclaredConstructors.All(x => x.IsPrivate).Should().BeTrue();
    }

    [Fact]
    public void BeCorrectWhenParamsAreCorrect()
    {
        //Arrange
        var orderId = Guid.NewGuid();
        var location = Location.Create(5, 5).Value;
        var volume = 10;

        //Act
        var result = Order.Create(orderId, location, volume);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Location.Should().Be(location);
        result.Value.Volume.Should().Be(volume);
    }

    [Theory]
    [MemberData(nameof(GetIncorrectOrderParams))]
    public void ReturnValueIsRequiredErrorWhenOrderIdIsEmpty(Guid orderId, Location location, int volume)
    {
        //Arrange

        var result = Order.Create(orderId, location, volume);

        //Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void CanAssignToCourier()
    {
        //Arrange
        var order = Order.Create(Guid.NewGuid(), Location.Create(5, 5).Value, 10).Value;
        var courier = Courier.Create("Pedestrian", 1, Location.Create(1, 1).Value).Value;

        //Act
        var result = order.Assign(courier);

        //Assert
        result.IsSuccess.Should().BeTrue();
        order.CourierId.Should().Be(courier.Id);
        order.Status.Should().Be(OrderStatus.Assigned);
    }

    [Fact]
    public void CanComplete()
    {
        //Arrange
        var order = Order.Create(Guid.NewGuid(), Location.Create(5, 5).Value, 10).Value;
        var courier = Courier.Create("Pedestrian", 1, Location.Create(1, 1).Value).Value;
        order.Assign(courier);

        //Act
        var result = order.Complete();

        //Assert
        result.IsSuccess.Should().BeTrue();
        order.CourierId.Should().Be(courier.Id);
        order.Status.Should().Be(OrderStatus.Completed);
    }

}
