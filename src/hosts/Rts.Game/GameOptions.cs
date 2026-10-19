namespace Rts.Game;

/// <summary>Command line options of the game host.</summary>
/// <param name="ScenarioFile">Plays the map and commands of a scenario file instead of the default map.</param>
/// <param name="ScreenshotFile">Saves a screenshot after a short warm-up and exits; for checking the rendering.</param>
/// <param name="IsDebugOverlayVisible">Starts with the F3 debug overlay switched on.</param>
internal sealed record GameOptions(string? ScenarioFile, string? ScreenshotFile, bool IsDebugOverlayVisible)
{
    public const string Usage = "Usage: Rts.Game [--scenario <file.scenario>] [--screenshot <file.png>] [--debug]";

    /// <returns>The options, or null if the arguments are invalid.</returns>
    public static GameOptions? Parse(IReadOnlyList<string> args)
    {
        string? scenario = null;
        string? screenshot = null;
        var isDebug = false;

        for (var index = 0; index < args.Count; index++)
        {
            var hasValue = index + 1 < args.Count;

            switch (args[index])
            {
                case "--scenario" when hasValue:
                    scenario = args[++index];
                    break;
                case "--screenshot" when hasValue:
                    screenshot = args[++index];
                    break;
                case "--debug":
                    isDebug = true;
                    break;
                default:
                    return null;
            }
        }

        return new GameOptions(scenario, screenshot, isDebug);
    }
}
