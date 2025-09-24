using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryApp.Core.Domain.Services;

public class DispatchService : IDispatchService
{
    public Result<Courier, Error> Dispatch(Order pendingOrder, IList<Courier> availableCouriers)
    {
        if (pendingOrder is null) return GeneralErrors.ValueIsRequired(nameof(pendingOrder));
        if (pendingOrder.Status != OrderStatus.Created) return Errors.OnlyOrderWithStatusCreatedCanBeDispatched(pendingOrder);

        if (availableCouriers is null) return GeneralErrors.ValueIsRequired(nameof(availableCouriers));
        if (availableCouriers.Count is 0) return GeneralErrors.CollectionIsTooSmall(1, 0);

        var mostSuitableCourier = availableCouriers
            .Where(courier => courier.CanTakeOrder(pendingOrder).Value)
            .MinBy(courier => courier.CalculateTimeToLocation(pendingOrder.Location).Value);

        if (mostSuitableCourier is null) return Errors.ErrNoAvailableCouriers();

        var assignResult = pendingOrder.Assign(mostSuitableCourier);
        if(assignResult.IsFailure) return assignResult.Error;

        var takeResult = mostSuitableCourier.TakeOrder(pendingOrder);
        if (takeResult.IsFailure) return takeResult.Error;

        return mostSuitableCourier;
    }

    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error ErrNoAvailableCouriers()
        {
            return new Error($"{nameof(Order).ToLowerInvariant()}.no.available.couriers",
                "Available couriers not found");
        }

        public static Error OnlyOrderWithStatusCreatedCanBeDispatched(Order order)
        {
            return new Error(
                "only.order.with.status.created.can.be.dispatched",
                $"Order (id: {order?.Id}, status: {order?.Status?.Name}) cannot be dispatched, only order with status \"Created\" can be"
                );
        }

    }
}


