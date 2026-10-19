using Rts.Game;

// Game host and composition root (ADR-0012): wires simulation, client and rendering, contains no game logic.
const int ExitSuccess = 0;
const int ExitInvalidInput = 2;

var options = GameOptions.Parse(args);

if (options is null)
{
    Console.Error.WriteLine(GameOptions.Usage);
    return ExitInvalidInput;
}

GameSession session;

try
{
    session = GameSession.Create(options.ScenarioFile);
}
catch (Exception error) when (error is FormatException or IOException or UnauthorizedAccessException)
{
    Console.Error.WriteLine(error.Message);
    return ExitInvalidInput;
}

new GameWindow(session, options).Run();
return ExitSuccess;
