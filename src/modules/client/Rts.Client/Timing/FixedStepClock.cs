namespace Rts.Client.Timing;

/// <summary>Result of one frame: how many simulation ticks to run and how far rendering is into the next tick.</summary>
/// <param name="Alpha">Interpolation factor in [0, 1] between the previous and the current snapshot (ADR-0007).</param>
public readonly record struct ClockFrame(int Ticks, float Alpha, FixedStepClock Next);

/// <summary>
/// Accumulator that decouples the fixed simulation tick from the frame rate (ADR-0007). Immutable.
/// Wall-clock time only decides when ticks run, never what they compute, so determinism is unaffected.
/// </summary>
public readonly record struct FixedStepClock(double AccumulatedSeconds)
{
    /// <summary>
    /// Upper bound of ticks per frame. After a frame spike (window dragged, debugger break) the backlog is
    /// worked off over several frames instead of freezing one frame for long ("spiral of death", PRD E13).
    /// </summary>
    public const int MaxTicksPerFrame = 5;

    public static FixedStepClock Start => new(0);

    /// <exception cref="ArgumentOutOfRangeException">A duration is negative, or the tick duration is not positive.</exception>
    public ClockFrame Advance(double frameSeconds, double tickSeconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(frameSeconds);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tickSeconds);

        var accumulated = AccumulatedSeconds + frameSeconds;
        var ticks = (int)Math.Min(Math.Floor(accumulated / tickSeconds), MaxTicksPerFrame);
        accumulated -= ticks * tickSeconds;
        var alpha = (float)Math.Min(accumulated / tickSeconds, 1.0);

        return new ClockFrame(ticks, alpha, new FixedStepClock(accumulated));
    }
}
