namespace Rts.Simulation;

/// <summary>
/// FNV-1a (64 bit) over explicitly fed integers. Never use <see cref="object.GetHashCode"/> for state hashes:
/// it is randomized per process and differs between runtimes, which would break desync detection (ADR-0004).
/// </summary>
internal sealed class StateHasher
{
    private const ulong OffsetBasis = 14695981039346656037;
    private const ulong Prime = 1099511628211;
    private const int BitsPerByte = 8;
    private const int BytesPerLong = sizeof(long);
    private const ulong ByteMask = 0xFF;

    public ulong Value { get; private set; } = OffsetBasis;

    public void Add(long value)
    {
        // Byte order is fixed (little endian) so the hash does not depend on the platform.
        for (var i = 0; i < BytesPerLong; i++)
        {
            Value ^= ((ulong)value >> (i * BitsPerByte)) & ByteMask;
            Value *= Prime;
        }
    }
}
