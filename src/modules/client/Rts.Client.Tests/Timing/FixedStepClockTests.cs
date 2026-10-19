using Rts.Client.Timing;

namespace Rts.Client.Tests.Timing;

public sealed class FixedStepClockTests
{
    private const double TickSeconds = 0.05;
    private const float AlphaTolerance = 1e-6f;

    [Fact]
    public void Advance_LessThanOneTick_RunsNoTickAndReportsProgress()
    {
        var frame = FixedStepClock.Start.Advance(0.02, TickSeconds);

        Assert.Equal(0, frame.Ticks);
        Assert.Equal(0.4f, frame.Alpha, AlphaTolerance);
    }

    [Fact]
    public void Advance_AccumulatesAcrossFrames()
    {
        var first = FixedStepClock.Start.Advance(0.03, TickSeconds);
        var second = first.Next.Advance(0.03, TickSeconds);

        Assert.Equal(0, first.Ticks);
        Assert.Equal(1, second.Ticks);
        Assert.Equal(0.2f, second.Alpha, AlphaTolerance);
    }

    [Fact]
    public void Advance_FrameSpike_RunsAtMostMaxTicksAndCatchesUpLater()
    {
        const double twoSeconds = 2.0;

        var spike = FixedStepClock.Start.Advance(twoSeconds, TickSeconds);
        var catchUp = spike.Next.Advance(0, TickSeconds);

        Assert.Equal(FixedStepClock.MaxTicksPerFrame, spike.Ticks);
        Assert.Equal(1f, spike.Alpha);
        Assert.Equal(FixedStepClock.MaxTicksPerFrame, catchUp.Ticks);
    }

    [Fact]
    public void Advance_ReturnsNewClockAndKeepsOriginal()
    {
        var clock = FixedStepClock.Start;

        clock.Advance(1.0, TickSeconds);

        Assert.Equal(FixedStepClock.Start, clock);
    }

    [Theory]
    [InlineData(-0.01, TickSeconds)]
    [InlineData(0.01, 0.0)]
    public void Advance_InvalidDurations_Throw(double frameSeconds, double tickSeconds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FixedStepClock.Start.Advance(frameSeconds, tickSeconds));
    }
}
