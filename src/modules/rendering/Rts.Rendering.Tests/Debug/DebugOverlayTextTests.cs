using Rts.Rendering.Debug;
using Rts.Simulation.Contracts;

namespace Rts.Rendering.Tests.Debug;

public sealed class DebugOverlayTextTests
{
    [Fact]
    public void Lines_ShowTickRatesHashRejectionsAndEntityCount()
    {
        var snapshot = new WorldSnapshot(Tick: 1234, StateHash: 0x797755843c6f523, RejectedCommandCount: 2, Entities: []);

        var lines = DebugOverlayText.Lines(snapshot, new FrameStats(FramesPerSecond: 60, TicksPerSecond: 20));

        Assert.Equal<string>(
            [
                "Tick 1234",
                "FPS 60 | TPS 20",
                "Hash 0797755843c6f523",
                "Rejected commands 2",
                "Entities 0",
            ],
            lines);
    }
}
