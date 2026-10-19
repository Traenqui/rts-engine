namespace Rts.Primitives;

/// <summary>Deterministic 2D vector of <see cref="Fixed"/> components in world units (1 tile = 1.0).</summary>
public readonly record struct FixedVector2(Fixed X, Fixed Y)
{
    // Small vectors are scaled up to at least this many significant bits before normalizing,
    // otherwise truncation of the components dominates the result.
    private const int NormalizePrecisionBits = 32;

    /// <summary>The zero vector.</summary>
    public static FixedVector2 Zero => new(Fixed.Zero, Fixed.Zero);

    /// <summary>Squared length. Can overflow for vectors longer than ~46 000 units.</summary>
    public Fixed LengthSquared() => X * X + Y * Y;

    /// <summary>Length, floored. Computed on raw values so neither tiny nor huge vectors lose precision.</summary>
    public Fixed Length()
    {
        return Fixed.FromRaw((long)RawLength(X.Raw, Y.Raw));
    }

    /// <summary>Unit vector in the same direction, truncated toward zero; the zero vector stays zero.</summary>
    public FixedVector2 Normalize()
    {
        if (X == Fixed.Zero && Y == Fixed.Zero)
        {
            return Zero;
        }

        // Scaling both components by the same power of two is exact and keeps the direction unchanged.
        var largestComponent = Math.Max(Math.Abs(X.Raw), Math.Abs(Y.Raw));
        var shift = Math.Max(0, NormalizePrecisionBits - (64 - (int)long.LeadingZeroCount(largestComponent)));
        var scaledX = (Int128)X.Raw << shift;
        var scaledY = (Int128)Y.Raw << shift;
        var length = (Int128)RawLength(scaledX, scaledY);

        return new(
            Fixed.FromRaw((long)((scaledX << Fixed.FractionalBits) / length)),
            Fixed.FromRaw((long)((scaledY << Fixed.FractionalBits) / length)));
    }

    public static FixedVector2 operator +(FixedVector2 a, FixedVector2 b) => new(a.X + b.X, a.Y + b.Y);

    public static FixedVector2 operator -(FixedVector2 a, FixedVector2 b) => new(a.X - b.X, a.Y - b.Y);

    public static FixedVector2 operator -(FixedVector2 value) => new(-value.X, -value.Y);

    public static FixedVector2 operator *(FixedVector2 vector, Fixed scalar) => new(vector.X * scalar, vector.Y * scalar);

    public static FixedVector2 operator /(FixedVector2 vector, Fixed scalar) => new(vector.X / scalar, vector.Y / scalar);

    private static UInt128 RawLength(Int128 x, Int128 y)
    {
        var absX = (UInt128)Int128.Abs(x);
        var absY = (UInt128)Int128.Abs(y);
        return IntegerMath.FloorSqrt(absX * absX + absY * absY);
    }
}
