using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Primitives;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryApp.Core.Domain.Model.OrderAggregate;

public sealed class Order : Aggregate<Guid>
{
    [ExcludeFromCodeCoverage]
    private Order()
    {
    }

    private Order(Guid orderId, Location location, int volume) : this()
    {
        Id = orderId;
        Location = location;
        Volume = volume;
        Status = OrderStatus.Created;
    }

    public Location Location { get; private set; }

    public int Volume { get; private set; }

    public OrderStatus Status { get; private set; }

    public Guid? CourierId { get; private set; }

    public static Result<Order, Error> Create(Guid orderId, Location location, int volume)
    {
        if (orderId == Guid.Empty) return GeneralErrors.ValueIsRequired(nameof(orderId));
        if (location == null) return GeneralErrors.ValueIsRequired(nameof(location));
        if (volume <= 0) return GeneralErrors.ValueIsRequired(nameof(volume));

        return new Order(orderId, location, volume);
    }

    public UnitResult<Error> Assign(Courier courier)
    {
        if (courier == null) return GeneralErrors.ValueIsRequired(nameof(courier));
        if (Status != OrderStatus.Created) return Errors.ErrCantAssignAlreadyAssignedOrder(courier.Id);

        CourierId = courier.Id;
        Status = OrderStatus.Assigned;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Complete()
    {
        if (Status != OrderStatus.Assigned) return Errors.ErrCantCompleteNotAssignedOrder();
        if (CourierId == null) return Errors.ErrCantCompleteNotAssignedOrder();

        Status = OrderStatus.Completed;
        return UnitResult.Success<Error>();
    }

    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error ErrCantCompleteNotAssignedOrder()
        {
            return new Error($"{nameof(Order).ToLowerInvariant()}.cant.complete.not.assigned.order",
                "Нельзя завершить заказ, который не был назначен");
        }

        public static Error ErrCantAssignAlreadyAssignedOrder(Guid courierId)
        {
            return new Error($"{nameof(Order).ToLowerInvariant()}.cant.assign.already.assigned.order",
                $"Нельзя назначить уже назначенный заказ {courierId} ");
        }
    }

}
