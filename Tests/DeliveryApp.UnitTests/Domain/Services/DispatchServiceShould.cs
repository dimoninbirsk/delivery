using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using DeliveryApp.Core.Domain.Services;

namespace DeliveryApp.UnitTests.Domain.Services;

public class DispatchServiceShould
{
    [Fact]
    public void FindNearestCourierForOrder()
    {
        // Arrange
        var courier1 = Courier.Create("Alex", 1, Location.Create(1, 1).Value).Value;
        var courier2 = Courier.Create("Brian", 1, Location.Create(5, 5).Value).Value;
        var courier3 = Courier.Create("Connor", 1, Location.Create(10, 10).Value).Value;
        List<Courier> couriers = [courier1, courier2, courier3];

        var order = Order.Create(Guid.NewGuid(), Location.Create(4, 6).Value, 5).Value;

        // Act
        var dispatchService = new DispatchService();
        var result = dispatchService.Dispatch(order, couriers);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var winner = result.Value;
        winner.Should().Be(courier2);
        winner.StoragePlaces.Should().Contain(c => c.OrderId == order.Id);
        order.CourierId.Should().Be(courier2.Id);
    }

    [Fact]
    public void FindFastestCourier()
    {
        // Arrange
        var courier1 = Courier.Create("Human", 1, Location.Create(5, 5).Value).Value;
        var courier2 = Courier.Create("Bicycle", 2, Location.Create(5, 5).Value).Value;
        var courier3 = Courier.Create("Car", 3, Location.Create(5, 5).Value).Value;
        List<Courier> couriers = [courier1, courier2, courier3];

        var order = Order.Create(Guid.NewGuid(), Location.Create(4, 4).Value, 5).Value;

        // Act
        var dispatchService = new DispatchService();
        var result = dispatchService.Dispatch(order, couriers);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(courier3);
    }

    [Fact]
    public void ReturnValueIsRequiredErrorWhenCouriersListIsEmpty()
    {
        // Arrange
        var couriers = new List<Courier>();
        var order = Order.Create(Guid.NewGuid(), Location.Create(2, 2).Value, 5).Value;

        // Act
        var dispatchService = new DispatchService();
        var result = dispatchService.Dispatch(order, couriers);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void ReturnErrorWhenNoAvailableCouriers()
    {
        // Arrange
        var courier1 = Courier.Create("Alex", 1, Location.Create(1, 1).Value).Value;
        var courier2 = Courier.Create("Brian", 1, Location.Create(5, 5).Value).Value;
        var courier3 = Courier.Create("Connor", 1, Location.Create(10, 10).Value).Value;
        List<Courier> couriers = [courier1, courier2, courier3];
        var order = Order.Create(Guid.NewGuid(), Location.Create(2, 2).Value, 12).Value;

        // Act
        var dispatchService = new DispatchService();
        var result = dispatchService.Dispatch(order, couriers);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(DispatchService.Errors.ErrNoAvailableCouriers());
    }

    [Fact]
    public void ReturnValueIsRequiredErrorWhenOrderIsNull()
    {
        // Arrange
        var courier = Courier.Create("Bicycle", 1, Location.Create(5, 5).Value).Value;
        List<Courier> couriers = [courier];

        // Act
        var dispatchService = new DispatchService();
        var result = dispatchService.Dispatch(null, couriers);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }
}
