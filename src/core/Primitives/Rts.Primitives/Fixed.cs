using System.Globalization;

namespace Rts.Primitives;

/// <summary>
/// Deterministic Q48.16 fixed-point number stored in a <see cref="long"/> (see ADR-0005).
/// All operations that drop fractional bits truncate toward zero.
/// Overflow throws in Debug builds and wraps in Release builds.
/// </summary>
public readonly struct Fixed : IEquatable<Fixed>, IComparable<Fixed>
{
    /// <summary>Number of fractional bits.</summary>
    public const int FractionalBits = 16;

    private const long OneRaw = 1L << FractionalBits;

    private Fixed(long raw)
    {
        Raw = raw;
    }

    /// <summary>The underlying raw value, i.e. the number multiplied by 2^16.</summary>
    public long Raw { get; }

    /// <summary>The value 0.</summary>
    public static Fixed Zero => new(0);

    /// <summary>The value 1.</summary>
    public static Fixed One => new(OneRaw);

    /// <summary>Creates a value from its raw representation.</summary>
    public static Fixed FromRaw(long raw) => new(raw);

    /// <summary>Creates a value from an integer.</summary>
    public static Fixed FromInt(int value) => new(value * OneRaw);

    /// <summary>
    /// Creates <paramref name="numerator"/> / <paramref name="denominator"/>, truncated toward zero.
    /// The deterministic way to express fractional constants such as 0.04 as <c>FromRatio(1, 25)</c>.
    /// </summary>
    /// <exception cref="DivideByZeroException"><paramref name="denominator"/> is zero.</exception>
    public static Fixed FromRatio(long numerator, long denominator) => new(numerator * OneRaw / denominator);

    /// <summary>Absolute value.</summary>
    public static Fixed Abs(Fixed value) => new(Math.Abs(value.Raw));

    /// <summary>The smaller of two values.</summary>
    public static Fixed Min(Fixed a, Fixed b) => a.Raw <= b.Raw ? a : b;

    /// <summary>The larger of two values.</summary>
    public static Fixed Max(Fixed a, Fixed b) => a.Raw >= b.Raw ? a : b;

    /// <summary>Square root, floored to the nearest representable value below the exact root.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
    public static Fixed Sqrt(Fixed value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value.Raw, nameof(value));

        // sqrt(raw / 2^16) * 2^16 == sqrt(raw * 2^16); UInt128 keeps the shifted input from overflowing.
        var root = IntegerMath.FloorSqrt((UInt128)value.Raw << FractionalBits);
        return new((long)root);
    }

    /// <summary>Largest integer less than or equal to this value, e.g. the tile index of a world coordinate.</summary>
    public int FloorToInt() => (int)(Raw >> FractionalBits);

    /// <summary>Converts to <see cref="float"/>. Only for rendering, never feed the result back into the simulation.</summary>
    public float ToSingle() => (float)Raw / OneRaw;

    public static Fixed operator +(Fixed a, Fixed b) => new(a.Raw + b.Raw);

    public static Fixed operator -(Fixed a, Fixed b) => new(a.Raw - b.Raw);

    public static Fixed operator -(Fixed value) => new(-value.Raw);

    public static Fixed operator *(Fixed a, Fixed b) => new(a.Raw * b.Raw / OneRaw);

    /// <exception cref="DivideByZeroException"><paramref name="b"/> is zero.</exception>
    public static Fixed operator /(Fixed a, Fixed b) => new(a.Raw * OneRaw / b.Raw);

    public static bool operator ==(Fixed a, Fixed b) => a.Raw == b.Raw;

    public static bool operator !=(Fixed a, Fixed b) => a.Raw != b.Raw;

    public static bool operator <(Fixed a, Fixed b) => a.Raw < b.Raw;

    public static bool operator >(Fixed a, Fixed b) => a.Raw > b.Raw;

    public static bool operator <=(Fixed a, Fixed b) => a.Raw <= b.Raw;

    public static bool operator >=(Fixed a, Fixed b) => a.Raw >= b.Raw;

    /// <inheritdoc />
    public bool Equals(Fixed other) => Raw == other.Raw;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Fixed other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Raw.GetHashCode();

    /// <inheritdoc />
    public int CompareTo(Fixed other) => Raw.CompareTo(other.Raw);

    /// <inheritdoc />
    public override string ToString() => ((double)Raw / OneRaw).ToString(CultureInfo.InvariantCulture);
}
