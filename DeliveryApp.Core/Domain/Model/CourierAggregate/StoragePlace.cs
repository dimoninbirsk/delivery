using CSharpFunctionalExtensions;
using Primitives;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

public class StoragePlace : Entity<Guid>
{
    public string Name { get; private set; }
    public int Volume { get; private set; }
    public Guid? OrderId { get; private set; }

    [ExcludeFromCodeCoverage]
    private StoragePlace()
    {
    }

    private StoragePlace(string name, int volume) : base()
    {
        Id = Guid.NewGuid();

        this.Name = name;
        this.Volume = volume;
    }

    public static Result<StoragePlace, Error> Create(string name, int volume)
    {
        if (string.IsNullOrEmpty(name)) return GeneralErrors.ValueIsRequired(nameof(name));
        if (volume <= 0) return GeneralErrors.ValueIsInvalid(nameof(name));

        return new StoragePlace(name, volume);
    }

    public Result<bool, Error> CanStore(int volume)
    {
        if (volume <= 0) return GeneralErrors.ValueIsRequired(nameof(volume));
        if (IsOccupied()) return false;

        return Volume > volume;
    }

    public UnitResult<Error> Store(Guid orderId, int volume)
    {
        if (orderId == Guid.Empty) return GeneralErrors.ValueIsRequired(nameof(orderId));
        if (volume <= 0) return GeneralErrors.ValueIsInvalid(nameof(volume));

        var canStoreResult = CanStore(volume);
        if (canStoreResult.IsFailure) return canStoreResult.Error;

        var canStore = canStoreResult.Value;
        if (!canStore)
        {
            return Errors.ErrCannotStoreOrderInThisStoragePlace();
        }

        OrderId = orderId;
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Clear(Guid orderId)
    {
        if (orderId == Guid.Empty) return GeneralErrors.ValueIsRequired(nameof(orderId));
        if (OrderId != orderId) return Errors.ErrOrderNotStoredInThisPlace();

        OrderId = null;
        return UnitResult.Success<Error>();
    }

    private bool IsOccupied()
    {
        return OrderId.HasValue;
    }
}

[ExcludeFromCodeCoverage]
public static class Errors
{
    public static Error ErrCannotStoreOrderInThisStoragePlace()
    {
        return new Error($"{nameof(StoragePlace).ToLowerInvariant()}.cannot.store.order.in.this.storage.place",
            "Нельзя поместить заказ в это место хранения");
    }

    public static Error ErrOrderNotStoredInThisPlace()
    {
        return new Error($"{nameof(StoragePlace).ToLowerInvariant()}.order.is.not.stored.in.this.place",
            "В месте хранения нет заказа, который пытаются извлечь");
    }
}