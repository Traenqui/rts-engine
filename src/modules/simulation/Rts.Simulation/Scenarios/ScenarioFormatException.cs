namespace Rts.Simulation.Scenarios;

/// <summary>A scenario file is malformed. <see cref="Line"/> is 1-based and refers to the scenario file.</summary>
public sealed class ScenarioFormatException : FormatException
{
    public ScenarioFormatException(int line, string reason, Exception? innerException = null)
        : base($"Invalid scenario at line {line}: {reason}", innerException)
    {
        Line = line;
    }

    public int Line { get; }
}
