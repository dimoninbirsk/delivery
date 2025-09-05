using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.SharedKernel;

public class Location : ValueObject
{
    private const int LowerBoundaryX = 1;
    private const int LowerBoundaryY = 1;
    private const int UpperBoundaryX = 10;
    private const int UpperBoundaryY = 10;

    public int X { get; }
    public int Y { get; }

    [ExcludeFromCodeCoverage]
    private Location()
    {
    }

    private Location(int x, int y) : this()
    {
        X = x;
        Y = y;
    }

    public Result<int, Error> GetDistance(Location to)
    {
        if (to == null) return GeneralErrors.ValueIsRequired(nameof(to));

        return Math.Abs(this.X - to.X) + Math.Abs(this.Y - to.Y);
    }

    public static Result<Location, Error> Create(int x, int y)
    {
        if (x < LowerBoundaryX || x > UpperBoundaryX) return GeneralErrors.ValueIsInvalid(nameof(x));
        if (y < LowerBoundaryY || y > UpperBoundaryY) return GeneralErrors.ValueIsInvalid(nameof(x));

        return new Location(x, y);
    }

    public static Location GetRandomLocation() => new(Random.Shared.Next(LowerBoundaryX, UpperBoundaryX + 1), Random.Shared.Next(LowerBoundaryY, UpperBoundaryY + 1));

    [ExcludeFromCodeCoverage]
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return X;
        yield return Y;
    }
}