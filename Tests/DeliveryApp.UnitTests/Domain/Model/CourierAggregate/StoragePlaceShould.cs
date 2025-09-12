using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using FluentAssertions;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.CourierAggregate;

public class StoragePlaceShould
{
    [Fact]
    public void DerivedEntity()
    {
        //Arrange

        //Act
        var isDerivedEntity = typeof(StoragePlace).IsSubclassOf(typeof(Entity<Guid>));

        //Assert
        isDerivedEntity.Should().BeTrue();
    }

    [Fact]
    public void ConstructorShouldBePrivate()
    {
        // Arrange
        var typeInfo = typeof(StoragePlace).GetTypeInfo();

        // Act

        // Assert
        typeInfo.DeclaredConstructors.All(x => x.IsPrivate).Should().BeTrue();
    }

    [Fact]
    public void BeCorrectWhenParamsAreCorrect()
    {
        //Arrange

        //Act
        var result = StoragePlace.Create("Bag", 10);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Name.Should().Be("Bag");
        result.Value.Volume.Should().Be(10);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(9)]
    public void CanStore(int volume)
    {
        //Arrange
        var storagePlaceCreateResult = StoragePlace.Create("Сумка", 10);
        storagePlaceCreateResult.IsSuccess.Should().BeTrue();
        var storagePlace = storagePlaceCreateResult.Value;

        //Act
        var storagePlaceCanStoreResult = storagePlace.CanStore(volume);

        //Assert
        storagePlaceCanStoreResult.IsSuccess.Should().BeTrue();
        storagePlaceCanStoreResult.Value.Should().BeTrue();
    }

    [Fact]
    public void BeFalseWhenIdIsDifferent()
    {
        //Arrange

        //Act
        var bag = StoragePlace.Create("Bag", 10);
        var bag2 = StoragePlace.Create("Bag", 10);

        //Assert
        bag.Equals(bag2).Should().BeFalse();
    }

    [Fact]
    public void BeStoreOrderSuccessfully()
    {
        //Arrange
        var guid = Guid.NewGuid();
        //Act
        var bag = StoragePlace.Create("Bag", 10);
        bag.Value.Store(guid, 5);

        //Assert
        bag.Value.OrderId.Equals(guid).Should().BeTrue();
    }

    [Fact]
    public void BeClearOrderSuccessfully()
    {
        //Arrange
        var guid = Guid.NewGuid();
        //Act
        var bag = StoragePlace.Create("Bag", 10);
        bag.Value.Store(guid, 5);
        bag.Value.Clear(guid);

        //Assert
        bag.Value.OrderId.HasValue.Should().BeFalse();
    }
}