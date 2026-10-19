using System.Globalization;
using Rts.Simulation.Contracts;

namespace Rts.Rendering.Debug;

/// <summary>Measured client rates shown in the debug overlay.</summary>
public readonly record struct FrameStats(int FramesPerSecond, int TicksPerSecond);

/// <summary>Text lines of the F3 debug overlay (PRD US-09), kept separate from drawing so they are testable.</summary>
public static class DebugOverlayText
{
    private const string HashFormat = "x16";

    public static IReadOnlyList<string> Lines(WorldSnapshot snapshot, FrameStats stats)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var culture = CultureInfo.InvariantCulture;

        return
        [
            string.Create(culture, $"Tick {snapshot.Tick}"),
            string.Create(culture, $"FPS {stats.FramesPerSecond} | TPS {stats.TicksPerSecond}"),
            $"Hash {snapshot.StateHash.ToString(HashFormat, culture)}",
            string.Create(culture, $"Rejected commands {snapshot.RejectedCommandCount}"),
            string.Create(culture, $"Entities {snapshot.Entities.Length}"),
        ];
    }
}
