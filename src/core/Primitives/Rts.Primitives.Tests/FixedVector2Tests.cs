namespace Rts.Primitives.Tests;

public sealed class FixedVector2Tests
{
    private const long OneRaw = 1L << 16;
    private const long NormalizeToleranceRaw = 2;

    [Fact]
    public void Arithmetic_IsComponentWise()
    {
        var a = new FixedVector2(Fixed.FromInt(3), Fixed.FromInt(-2));
        var b = new FixedVector2(Fixed.FromRatio(1, 2), Fixed.FromInt(5));
        var two = Fixed.FromInt(2);

        Assert.Equal(new FixedVector2(Fixed.FromRatio(7, 2), Fixed.FromInt(3)), a + b);
        Assert.Equal(new FixedVector2(Fixed.FromRatio(5, 2), Fixed.FromInt(-7)), a - b);
        Assert.Equal(new FixedVector2(Fixed.FromInt(-3), Fixed.FromInt(2)), -a);
        Assert.Equal(new FixedVector2(Fixed.FromInt(6), Fixed.FromInt(-4)), a * two);
        Assert.Equal(new FixedVector2(Fixed.FromRatio(3, 2), Fixed.FromInt(-1)), a / two);
    }

    [Fact]
    public void LengthSquaredAndLength_ThreeFourFive_AreExact()
    {
        var vector = new FixedVector2(Fixed.FromInt(3), Fixed.FromInt(4));

        Assert.Equal(Fixed.FromInt(25), vector.LengthSquared());
        Assert.Equal(Fixed.FromInt(5), vector.Length());
    }

    [Fact]
    public void Normalize_ThreeFour_ReturnsUnitDirection()
    {
        var vector = new FixedVector2(Fixed.FromInt(3), Fixed.FromInt(4));

        var direction = vector.Normalize();

        Assert.Equal(Fixed.FromRatio(3, 5), direction.X);
        Assert.Equal(Fixed.FromRatio(4, 5), direction.Y);
    }

    [Theory]
    [InlineData(1L, 0L)]
    [InlineData(1L, 1L)]
    [InlineData(-123_456L, 789L)]
    [InlineData(100L * OneRaw, -37L * OneRaw)]
    public void Normalize_AnyNonZeroVector_HasUnitLength(long xRaw, long yRaw)
    {
        var vector = new FixedVector2(Fixed.FromRaw(xRaw), Fixed.FromRaw(yRaw));

        var length = vector.Normalize().Length();

        Assert.InRange(length.Raw, OneRaw - NormalizeToleranceRaw, OneRaw);
    }

    [Fact]
    public void Normalize_ZeroVector_ReturnsZero()
    {
        Assert.Equal(FixedVector2.Zero, FixedVector2.Zero.Normalize());
    }
}
