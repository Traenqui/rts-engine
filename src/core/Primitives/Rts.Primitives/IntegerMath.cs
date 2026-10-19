namespace Rts.Primitives;

internal static class IntegerMath
{
    private const int UInt128Bits = 128;

    /// <summary>
    /// Floor of the square root of <paramref name="value"/>, computed with integer Newton iteration only
    /// so the result is bit-identical on every platform.
    /// </summary>
    public static UInt128 FloorSqrt(UInt128 value)
    {
        if (value < 2)
        {
            return value;
        }

        // Start at a power of two that is guaranteed to be >= sqrt(value); Newton then decreases monotonically.
        var bitLength = UInt128Bits - (int)UInt128.LeadingZeroCount(value);
        var estimate = UInt128.One << ((bitLength + 1) / 2);

        while (true)
        {
            var next = (estimate + value / estimate) >> 1;

            if (next >= estimate)
            {
                return estimate;
            }

            estimate = next;
        }
    }
}
