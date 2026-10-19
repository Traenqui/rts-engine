using System.Reflection;
using Rts.Simulation;
using Rts.Simulation.Contracts;
using Rts.Simulation.Maps;
using Rts.Simulation.Rules;
using Rts.Simulation.Scenarios;

namespace Rts.Game;

/// <summary>The running simulation plus the two snapshots rendering interpolates between (ADR-0007).</summary>
internal sealed class GameSession
{
    private const string DefaultMapResource = "Rts.Game.Maps.default.map";

    private GameSession(GameSimulation simulation)
    {
        Simulation = simulation;
        Current = simulation.CreateSnapshot();
    }

    public GameSimulation Simulation { get; }

    public WorldSnapshot? Previous { get; private set; }

    public WorldSnapshot Current { get; private set; }

    /// <exception cref="FormatException">The scenario or map is malformed.</exception>
    /// <exception cref="IOException">The scenario file cannot be read.</exception>
    public static GameSession Create(string? scenarioFile)
    {
        if (scenarioFile is null)
        {
            return new GameSession(new GameSimulation(AsciiMapParser.Parse(ReadDefaultMap()), DefaultRules.Create()));
        }

        var scenario = ScenarioParser.Parse(File.ReadAllText(scenarioFile));
        var session = new GameSession(new GameSimulation(scenario.Map, DefaultRules.Create()));

        foreach (var command in scenario.Commands)
        {
            session.Simulation.Enqueue(command);
        }

        return session;
    }

    public void Step()
    {
        Simulation.Step();
        Previous = Current;
        Current = Simulation.CreateSnapshot();
    }

    private static string ReadDefaultMap()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(DefaultMapResource)
            ?? throw new InvalidOperationException($"Embedded map '{DefaultMapResource}' is missing from the build.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
