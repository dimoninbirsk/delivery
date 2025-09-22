using DeliveryApp.Core.Domain.Model.OrderAggregate;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.OrderAggregate;

public class OrderStatusShould
{
    [Fact]
    public void ReturnCorrectName()
    {
        //Arrange

        //Act

        //Assert
        OrderStatus.Created.Name.Should().Be("created");
        OrderStatus.Assigned.Name.Should().Be("assigned");
        OrderStatus.Completed.Name.Should().Be("completed");
    }

    [Fact]
    public void BeEqualWhenAllPropertiesIsEqual()
    {
        //Arrange

        //Act
        var result = OrderStatus.Created == OrderStatus.Created;

        //Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void BeNotEqualWhenAllPropertiesIsEqual()
    {
        //Arrange

        //Act
        var result = OrderStatus.Created == OrderStatus.Completed;

        //Assert
        result.Should().BeFalse();
    }

}
