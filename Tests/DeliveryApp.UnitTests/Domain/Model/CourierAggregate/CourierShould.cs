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

namespace DeliveryApp.UnitTests.Domain.Model.CourierAggregate;

public class CourierShould
{
    Location minLocation = Location.Create(1, 1).Value;
    Location maxLocation = Location.Create(10, 10).Value;

    [Fact]
    public void DerivedAggregate()
    {
        //Arrange

        //Act
        var isDerivedAggregate = typeof(Courier).IsSubclassOf(typeof(Aggregate<Guid>));

        //Assert
        isDerivedAggregate.Should().BeTrue();
    }

    [Fact]
    public void ConstructorShouldBePrivate()
    {
        // Arrange
        var typeInfo = typeof(Courier).GetTypeInfo();

        // Act

        // Assert
        typeInfo.DeclaredConstructors.All(x => x.IsPrivate).Should().BeTrue();
    }

    [Fact]
    public void BeCorrectWhenParamsAreCorrect()
    {
        //Arrange

        //Act
        var result = Courier.Create("Pedestrian", 1, minLocation);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Name.Should().Be("Pedestrian");
        result.Value.Speed.Should().Be(1);
        result.Value.StoragePlaces.Count.Should().Be(1);
        result.Value.Location.Should().Be(minLocation);
    }

    [Fact]
    public void ReturnValueIsRequiredErrorWhenNameIsEmpty()
    {
        //Arrange

        //Act
        var result = Courier.Create("", 1, minLocation);

        //Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(GeneralErrors.ValueIsRequired("name"));
    }
    [Fact]
    public void BeAbleToAddStorage()
    {
        //Arrange
        var courier = Courier.Create("Pedestrian", 1, minLocation).Value;

        //Act
        var result = courier.AddStorage("WaistBag", 2);

        //Assert
        result.IsSuccess.Should().BeTrue();
        courier.StoragePlaces.Should().HaveCount(2, 
            "Должно быть два места хранения: одно из конструктора и одно добавленное.");
        courier.StoragePlaces.Should().Contain(storage => storage.Name == "WaistBag" && storage.Volume == 2, 
            "Список хранилищ должен содержать добавленное место хранения с правильными свойствами.");
    }

    [Fact]
    public void BeAbleToTakeValidOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var courier = Courier.Create("Pedestrian", 1, minLocation).Value;
        var order = Order.Create(orderId, maxLocation, 5).Value; 

        // Act
        var result = courier.TakeOrder(order);

        // Assert
        result.IsSuccess.Should().BeTrue();
        courier.StoragePlaces.Should().Contain(item => item.OrderId == orderId);
    }

    [Fact]
    public void BeAbleToCompleteOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var courier = Courier.Create("Pedestrian", 1, minLocation).Value;
        var order = Order.Create(orderId, maxLocation, 5).Value;
        courier.TakeOrder(order);

        // Act

        var result = courier.CompleteOrder(order);

        // Assert
        result.IsSuccess.Should().BeTrue();
        courier.StoragePlaces.Should().NotContain(item => item.OrderId == orderId);
    }

    [Fact]
    public void ReturnsFalseWhenNoStorageFitsOrder()
    {
        // Arrange
        var courier = Courier.Create("Pedestrian", 1, minLocation).Value;
        var order = Order.Create(Guid.NewGuid(), maxLocation, 15).Value; // Заказ больше, чем любое хранилище

        // Act
        var result = courier.CanTakeOrder(order);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }


    [Fact]
    public void ChangeLocationAfterMove()
    {
        //Arrange
        var courier = Courier.Create("Pedestrian", 1, minLocation).Value;
        var targetLcation = Location.Create(2, 1).Value;

        //Act
        var result = courier.Move(maxLocation);

        //Assert
        result.IsSuccess.Should().BeTrue();
        courier.Location.Should().Be(targetLcation);
    }

    [Fact]
    public void CantMoveToIncorrectLocation()
    {
        //Arrange
        var courier = Courier.Create("Pedestrian", 1, minLocation).Value;

        //Act
        var result = courier.Move(null);

        //Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(GeneralErrors.ValueIsRequired("target"));
    }

    [Fact]
    public void CanCalculateTimeToLocation()
    {
        /*
        Изначальная точка курьера: [1,1]
        Целевая точка: [5,10]
        Количество шагов, необходимое курьеру: 13 (4 по горизонтали и 9 по вертикали)
        Скорость транспорта (пешехода): 1 шаг в 1 такт
        Время подлета: 13/13 = 13.0 тактов потребуется курьеру, чтобы доставить заказ
        */

        //Arrange
        var location = Location.Create(5, 10).Value;
        var courier = Courier.Create("Pedestrian", 1, minLocation).Value;

        //Act
        var result = courier.CalculateTimeToLocation(location);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(13);
    }

    [Theory]
    [MemberData(nameof(GetCouriersAndLocations))]
    public void CanMove(Courier courier, Location targetLocation, Location locationAfterMove)
    {
        //Arrange
        Console.WriteLine(courier.ToString());
        Console.WriteLine(targetLocation.ToString());
        Console.WriteLine(locationAfterMove.ToString());
        //Act
        var result = courier.Move(targetLocation);

        //Assert
        result.IsSuccess.Should().BeTrue();
        courier.Location.Should().Be(locationAfterMove);
    }

    public static IEnumerable<object[]> GetCouriersAndLocations()
    {
        // Пешеход, заказ X:совпадает, Y: совпадает
        yield return
        [
            Courier.Create("Pedestrian", 1, Location.Create(1, 1).Value).Value,
        Location.Create(1, 1).Value,
        Location.Create(1, 1).Value
        ];
        yield return
        [
            Courier.Create("Pedestrian", 1, Location.Create(5, 5).Value).Value,
        Location.Create(5, 5).Value,
        Location.Create(5, 5).Value
        ];

        // Пешеход, заказ X:совпадает, Y: выше
        yield return
        [
            Courier.Create("Pedestrian", 1, Location.Create(1, 1).Value).Value,
        Location.Create(1, 2).Value,
        Location.Create(1, 2).Value
        ];
        yield return
        [
            Courier.Create("Pedestrian", 1, Location.Create(1, 1).Value).Value,
        Location.Create(1, 5).Value,
        Location.Create(1, 2).Value
        ];

        // Пешеход, заказ X:правее, Y: совпадает
        yield return
        [
            Courier.Create("Pedestrian", 1, Location.Create(2, 2).Value).Value,
        Location.Create(3, 2).Value,
        Location.Create(3, 2).Value
        ];
        yield return
        [
            Courier.Create("Pedestrian", 1, Location.Create(5, 5).Value).Value,
        Location.Create(6, 5).Value,
        Location.Create(6, 5).Value
        ];

        // Пешеход, заказ X:правее, Y: выше
        yield return
        [
            Courier.Create("Pedestrian", 1,Location.Create(2, 2).Value).Value,
        Location.Create(3, 3).Value,
        Location.Create(3, 2).Value
        ];
        yield return
        [
            Courier.Create("Pedestrian", 1,Location.Create(1, 1).Value).Value,
        Location.Create(5, 5).Value,
        Location.Create(2, 1).Value
        ];

        // Пешеход, заказ X:совпадает, Y: ниже
        yield return
        [
            Courier.Create("Pedestrian", 1, Location.Create(1, 2).Value).Value,
        Location.Create(1, 1).Value,
        Location.Create(1, 1).Value
        ];
        yield return
        [
            Courier.Create("Pedestrian", 1, Location.Create(5, 5).Value).Value,
        Location.Create(5, 1).Value,
        Location.Create(5, 4).Value
        ];

        // Пешеход, заказ X:левее, Y: совпадает
        yield return
        [
            Courier.Create("Pedestrian", 1, Location.Create(2, 2).Value).Value,
        Location.Create(1, 2).Value,
        Location.Create(1, 2).Value
        ];
        yield return
        [
            Courier.Create("Pedestrian", 1, Location.Create(5, 5).Value).Value,
        Location.Create(1, 5).Value,
        Location.Create(4, 5).Value
        ];

        // Пешеход, заказ X:левее, Y: ниже
        yield return
        [
            Courier.Create("Pedestrian", 1, Location.Create(2, 2).Value).Value,
        Location.Create(1, 1).Value,
        Location.Create(1, 2).Value
        ];
        yield return
        [
            Courier.Create("Pedestrian", 1, Location.Create(5, 5).Value).Value,
        Location.Create(1, 1).Value,
        Location.Create(4, 5).Value
        ];


        // Велосипедист, заказ X:совпадает, Y: совпадает
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(1, 1).Value).Value,
        Location.Create(1, 1).Value,
        Location.Create(1, 1).Value
        ];
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(5, 5).Value).Value,
        Location.Create(5, 5).Value,
        Location.Create(5, 5).Value
        ];

        // Велосипедист, заказ X:совпадает, Y: выше
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(1, 1).Value).Value,
        Location.Create(1, 3).Value,
        Location.Create(1, 3).Value
        ];
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(1, 1).Value).Value,
        Location.Create(1, 5).Value,
        Location.Create(1, 3).Value
        ];

        // Велосипедист, заказ X:правее, Y: совпадает
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(2, 2).Value).Value,
        Location.Create(4, 2).Value,
        Location.Create(4, 2).Value
        ];
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(5, 5).Value).Value,
        Location.Create(8, 5).Value,
        Location.Create(7, 5).Value
        ];

        // Велосипедист, заказ X:правее, Y: выше
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(2, 2).Value).Value,
        Location.Create(4, 4).Value,
        Location.Create(4, 2).Value
        ];
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(1, 1).Value).Value,
        Location.Create(5, 5).Value,
        Location.Create(3, 1).Value
        ];

        // Велосипедист, заказ X:совпадает, Y: ниже
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(1, 3).Value).Value,
        Location.Create(1, 1).Value,
        Location.Create(1, 1).Value
        ];
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(5, 5).Value).Value,
        Location.Create(5, 1).Value,
        Location.Create(5, 3).Value
        ];

        // Велосипедист, заказ X:левее, Y: совпадает
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(3, 2).Value).Value,
        Location.Create(1, 2).Value,
        Location.Create(1, 2).Value
        ];
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(5, 5).Value).Value,
        Location.Create(1, 5).Value,
        Location.Create(3, 5).Value
        ];

        // Велосипедист, заказ X:левее, Y: ниже
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(3, 3).Value).Value,
        Location.Create(1, 1).Value,
        Location.Create(1, 3).Value
        ];
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(5, 5).Value).Value,
        Location.Create(1, 1).Value,
        Location.Create(3, 5).Value
        ];

        // Велосипедист, заказ ближе чем скорость
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(1, 1).Value).Value,
        Location.Create(1, 2).Value,
        Location.Create(1, 2).Value
        ];
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(1, 1).Value).Value,
        Location.Create(2, 1).Value,
        Location.Create(2, 1).Value
        ];
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(5, 5).Value).Value,
        Location.Create(5, 4).Value,
        Location.Create(5, 4).Value
        ];
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(5, 5).Value).Value,
        Location.Create(4, 5).Value,
        Location.Create(4, 5).Value
        ];

        // Велосипедист, заказ с шагами по 2 осям
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(1, 1).Value).Value,
        Location.Create(2, 2).Value,
        Location.Create(2, 2).Value
        ];
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(5, 5).Value).Value,
        Location.Create(4, 4).Value,
        Location.Create(4, 4).Value
        ];
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(1, 1).Value).Value,
        Location.Create(1, 2).Value,
        Location.Create(1, 2).Value
        ];
        yield return
        [
            Courier.Create("Bicycle", 2, Location.Create(5, 5).Value).Value,
        Location.Create(5, 4).Value,
        Location.Create(5, 4).Value
        ];
    }


}

