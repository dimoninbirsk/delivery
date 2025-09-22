using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Primitives;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

public sealed class Courier : Aggregate<Guid>
{
    [ExcludeFromCodeCoverage]
    private Courier()
    {
    }

    private Courier(string name, int speed, Location location, StoragePlace storagePlace) : this()
    {
        Id = Guid.NewGuid();
        this.Name = name;
        this.Speed = speed;
        this.Location = location;
       StoragePlaces.Add(storagePlace);

    }

    public string Name { get; private set; }
    public int Speed { get; private set; }
    public Location Location { get; private set; }
    public List<StoragePlace> StoragePlaces { get; private set; } = [];

    public static Result<Courier, Error> Create(string name, int speed, Location location)
    {
        if (string.IsNullOrEmpty(name)) return GeneralErrors.ValueIsRequired(nameof(name));
        if (speed <= 0) return GeneralErrors.ValueIsRequired(nameof(speed));
        if (location == null) return GeneralErrors.ValueIsRequired(nameof(location));

        var (_, isFailure, storagePlace, error) = StoragePlace.Create("Bag", 10);

        if (isFailure) return error;

        return new Courier(name, speed, location, storagePlace);
    }

    public UnitResult<Error> AddStorage(string name, int value)
    {
        var newStorage = StoragePlace.Create(name, value);

        if (newStorage.IsFailure) return newStorage.Error;

        var storage = newStorage.Value;
        StoragePlaces.Add(storage);

        return UnitResult.Success<Error>();
    }

    public Result<bool, Error> CanTakeOrder(Order order)
    {
        if (order == null) return GeneralErrors.ValueIsRequired(nameof(order));

        foreach (var canStoreResult in StoragePlaces.Select(storagePlace => storagePlace.CanStore(order.Volume)))
        {
            if (canStoreResult.IsFailure) return canStoreResult.Error;
            var canStore = canStoreResult.Value;
            if (canStore) return true;
        }

        return false;
    }

    public UnitResult<Error> TakeOrder(Order order)
    {
        if (order == null) return GeneralErrors.ValueIsRequired(nameof(order));

        foreach (var storagePlace in StoragePlaces)
        {
            var canStoreResult = storagePlace.CanStore(order.Volume);
            if (canStoreResult.IsFailure) return canStoreResult.Error;
            var canStore = canStoreResult.Value;
            if (canStore)
            {
                var storagePlaceStoreResult = storagePlace.Store(order.Id, order.Volume);
                if (storagePlaceStoreResult.IsFailure) return storagePlaceStoreResult.Error;
                return UnitResult.Success<Error>();
            }
        }

        return Errors.ErrNoSuitableStoragePlace();
    }

    public UnitResult<Error> CompleteOrder(Order order)
    {
        if (order == null) return GeneralErrors.ValueIsRequired(nameof(order));
        var storagePlace = StoragePlaces.SingleOrDefault(c => c.OrderId == order.Id);
        if (storagePlace != null)
        {
            var storagePlaceClearResult = storagePlace.Clear(order.Id);
            if (storagePlaceClearResult.IsFailure) return storagePlaceClearResult.Error;
        }
        return UnitResult.Success<Error>();
    }

    public Result<double, Error> CalculateTimeToLocation(Location location)
    {
        if (location == null) return GeneralErrors.ValueIsRequired(nameof(location));

        var distanceToResult = Location.GetDistance(location);
        if (distanceToResult.IsFailure) return distanceToResult.Error;
        var distance = distanceToResult.Value;

        var time = (double)distance / Speed;
        return time;
    }

    public UnitResult<Error> Move(Location target)
    {
        if (target == null) return GeneralErrors.ValueIsRequired(nameof(target));

        var difX = target.X - Location.X;
        var difY = target.Y - Location.Y;
        var cruisingRange = Speed;

        var moveX = Math.Clamp(difX, -cruisingRange, cruisingRange);
        cruisingRange -= Math.Abs(moveX);

        var moveY = Math.Clamp(difY, -cruisingRange, cruisingRange);

        var locationCreateResult = Location.Create(Location.X + moveX, Location.Y + moveY);
        if (locationCreateResult.IsFailure) return locationCreateResult.Error;
        Location = locationCreateResult.Value;

        return UnitResult.Success<Error>();
    }


    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error ErrNoSuitableStoragePlace()
        {
            return new Error($"{nameof(Courier).ToLowerInvariant()}.no.suitable.storage.place",
                "Нельзя взять заказ, нет свободных мест");
        }
    }

}
