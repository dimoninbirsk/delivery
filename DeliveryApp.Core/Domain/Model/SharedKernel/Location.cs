using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;

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

    public Location(int x, int y) : this()
    {
        if (x < LowerBoundaryX || x > UpperBoundaryX) throw new ArgumentOutOfRangeException(nameof(x));
        if (y < LowerBoundaryY || y > UpperBoundaryY) throw new ArgumentOutOfRangeException(nameof(y));

        X = x;
        Y = y;
    }

    public static int GetDistance(Location from, Location to)
    {
        return Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
    }

    public static Location GetRandomLocation() => new(Random.Shared.Next(1, 11), Random.Shared.Next(1, 11));

    [ExcludeFromCodeCoverage]
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return X;
        yield return Y;
    }
}