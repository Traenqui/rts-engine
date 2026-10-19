namespace Rts.Primitives.Tests;

public sealed class FixedTests
{
    private const long OneRaw = 1L << 16;

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-7)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void FromInt_StoresValueInUpperBits(int value)
    {
        var fixedValue = Fixed.FromInt(value);

        Assert.Equal(value * OneRaw, fixedValue.Raw);
    }

    [Theory]
    [InlineData(1, 4, 16384)]
    [InlineData(1, 25, 2621)]
    [InlineData(-1, 3, -21845)]
    [InlineData(1, -3, -21845)]
    [InlineData(5, 2, 163840)]
    public void FromRatio_TruncatesTowardZero(long numerator, long denominator, long expectedRaw)
    {
        var fixedValue = Fixed.FromRatio(numerator, denominator);

        Assert.Equal(expectedRaw, fixedValue.Raw);
    }

    [Fact]
    public void FromRatio_ZeroDenominator_Throws()
    {
        Assert.Throws<DivideByZeroException>(() => Fixed.FromRatio(1, 0));
    }

    [Fact]
    public void AddAndSubtract_AreExact()
    {
        var a = Fixed.FromRatio(3, 2);
        var b = Fixed.FromRatio(1, 4);

        Assert.Equal(Fixed.FromRatio(7, 4), a + b);
        Assert.Equal(Fixed.FromRatio(5, 4), a - b);
        Assert.Equal(Fixed.FromRatio(-3, 2), -a);
    }

    [Theory]
    [InlineData(3, 2, 2, 1, 3, 1)]
    [InlineData(1, 2, 1, 2, 1, 4)]
    [InlineData(-3, 1, 1, 4, -3, 4)]
    public void Multiply_ExactValues_ReturnsProduct(long an, long ad, long bn, long bd, long en, long ed)
    {
        var product = Fixed.FromRatio(an, ad) * Fixed.FromRatio(bn, bd);

        Assert.Equal(Fixed.FromRatio(en, ed), product);
    }

    [Theory]
    [InlineData(1, 1, 0)]
    [InlineData(3, 32768, 1)]
    [InlineData(-3, 32768, -1)]
    [InlineData(3, -32768, -1)]
    public void Multiply_FractionalRemainder_TruncatesTowardZero(long aRaw, long bRaw, long expectedRaw)
    {
        var product = Fixed.FromRaw(aRaw) * Fixed.FromRaw(bRaw);

        Assert.Equal(expectedRaw, product.Raw);
    }

    [Fact]
    public void Multiply_IsCommutative()
    {
        var a = Fixed.FromRatio(-17, 3);
        var b = Fixed.FromRatio(5, 7);

        Assert.Equal(a * b, b * a);
    }

    [Fact]
    public void Multiply_MaxMapDistanceSquared_DoesNotOverflow()
    {
        const int maxMapDistance = 362;
        var distance = Fixed.FromInt(maxMapDistance);

        var squared = distance * distance;

        Assert.Equal(Fixed.FromInt(maxMapDistance * maxMapDistance), squared);
    }

#if DEBUG
    // Overflow checks are only compiled into Debug builds of Rts.Primitives (ADR-0005).
    [Fact]
    public void Multiply_ResultOutOfRange_ThrowsInDebugBuilds()
    {
        const int hugeValue = 50_000;
        var huge = Fixed.FromInt(hugeValue);

        Assert.Throws<OverflowException>(() => huge * huge);
    }
#endif

    [Theory]
    [InlineData(1, 4, 16384)]
    [InlineData(-1, 2, -32768)]
    [InlineData(10, 4, 163840)]
    public void Divide_ReturnsQuotient(int a, int b, long expectedRaw)
    {
        var quotient = Fixed.FromInt(a) / Fixed.FromInt(b);

        Assert.Equal(expectedRaw, quotient.Raw);
    }

    [Theory]
    [InlineData(1, 2, 0)]
    [InlineData(-1, 2, 0)]
    [InlineData(-3, 2, -1)]
    public void Divide_FractionalRemainder_TruncatesTowardZero(long aRaw, int b, long expectedRaw)
    {
        var quotient = Fixed.FromRaw(aRaw) / Fixed.FromInt(b);

        Assert.Equal(expectedRaw, quotient.Raw);
    }

    [Fact]
    public void Divide_ByZero_Throws()
    {
        Assert.Throws<DivideByZeroException>(() => Fixed.One / Fixed.Zero);
    }

    [Fact]
    public void Comparison_OrdersByValue()
    {
        var small = Fixed.FromRatio(-1, 2);
        var large = Fixed.FromRatio(1, 3);
        var sameAsSmall = Fixed.FromRatio(-1, 2);

        Assert.True(small < large);
        Assert.True(large > small);
        Assert.True(small <= sameAsSmall);
        Assert.True(sameAsSmall >= small);
        Assert.True(small != large);
        Assert.Equal(-1, small.CompareTo(large));
    }

    [Fact]
    public void Equality_ComparesRawValues()
    {
        var value = Fixed.FromRatio(5, 8);
        object boxedEqual = Fixed.FromRatio(5, 8);
        object boxedOther = Fixed.One;

        Assert.True(value.Equals(boxedEqual));
        Assert.False(value.Equals(boxedOther));
        Assert.False(value.Equals("0.625"));
        Assert.Equal(Fixed.FromRatio(5, 8).GetHashCode(), value.GetHashCode());
    }

    [Fact]
    public void ToString_UsesInvariantCulture()
    {
        Assert.Equal("-1.25", Fixed.FromRatio(-5, 4).ToString());
    }

    [Fact]
    public void AbsMinMax_ReturnExpectedValues()
    {
        var negative = Fixed.FromRatio(-5, 4);
        var positive = Fixed.FromRatio(3, 4);

        Assert.Equal(Fixed.FromRatio(5, 4), Fixed.Abs(negative));
        Assert.Equal(positive, Fixed.Abs(positive));
        Assert.Equal(negative, Fixed.Min(negative, positive));
        Assert.Equal(positive, Fixed.Max(negative, positive));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(4, 2)]
    [InlineData(9, 3)]
    [InlineData(131044, 362)]
    public void Sqrt_PerfectSquare_IsExact(int value, int expectedRoot)
    {
        var root = Fixed.Sqrt(Fixed.FromInt(value));

        Assert.Equal(Fixed.FromInt(expectedRoot), root);
    }

    [Fact]
    public void Sqrt_SmallestPositiveValue_ReturnsExactRoot()
    {
        const long expectedRaw = 256;

        var root = Fixed.Sqrt(Fixed.FromRaw(1));

        Assert.Equal(expectedRaw, root.Raw);
    }

    [Theory]
    [InlineData(2L)]
    [InlineData(65537L)]
    [InlineData(131072L)]
    [InlineData(123_456_789L)]
    [InlineData(1L << 47)]
    [InlineData(long.MaxValue)]
    public void Sqrt_AnyValue_ReturnsFlooredRoot(long raw)
    {
        var root = Fixed.Sqrt(Fixed.FromRaw(raw));

        var scaledInput = (UInt128)raw << 16;
        var rootRaw = (UInt128)root.Raw;
        Assert.True(rootRaw * rootRaw <= scaledInput, "root is too large");
        Assert.True((rootRaw + 1) * (rootRaw + 1) > scaledInput, "root is too small");
    }

    [Fact]
    public void Sqrt_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Fixed.Sqrt(Fixed.FromInt(-1)));
    }

    [Fact]
    public void ToSingle_ConvertsForRendering()
    {
        var value = Fixed.FromRatio(-3, 4);

        Assert.Equal(-0.75f, value.ToSingle());
    }
}
