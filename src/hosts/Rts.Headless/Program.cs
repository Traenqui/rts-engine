using Rts.Simulation.Rules;
using Rts.Simulation.Scenarios;

// Headless host (ADR-0013): plays scenario files without rendering and checks them against golden hashes.
// Exit codes: 0 = success, 1 = verification failed, 2 = invalid usage or input.
const int ExitSuccess = 0;
const int ExitVerificationFailed = 1;
const int ExitInvalidInput = 2;
const string ScenarioPattern = "*.scenario";
const string GoldenFileName = "golden-hashes.txt";

try
{
    return args switch
    {
        ["run", var file] => Run(file),
        ["print", var directory] => Print(directory),
        ["verify", var directory] => Verify(directory),
        _ => Usage(),
    };
}
catch (Exception error) when (error is FormatException or IOException or UnauthorizedAccessException)
{
    Console.Error.WriteLine(error.Message);
    return ExitInvalidInput;
}

static int Run(string file)
{
    Console.WriteLine(GoldenHashes.FormatHash(Play(file)));
    return ExitSuccess;
}

static int Print(string directory)
{
    Console.Write(GoldenHashes.Format(PlayAll(directory)));
    return ExitSuccess;
}

static int Verify(string directory)
{
    var expected = GoldenHashes.Parse(File.ReadAllText(Path.Combine(directory, GoldenFileName)));
    var results = GoldenHashes.Verify(expected, PlayAll(directory));

    foreach (var result in results)
    {
        var expectedHash = result.Expected is { } e ? GoldenHashes.FormatHash(e) : "-";
        var actualHash = result.Actual is { } a ? GoldenHashes.FormatHash(a) : "-";
        Console.WriteLine($"{result.Status,-15} {result.Name} expected={expectedHash} actual={actualHash}");
    }

    return results.All(result => result.Status == VerificationStatus.Match) ? ExitSuccess : ExitVerificationFailed;
}

static Dictionary<string, ulong> PlayAll(string directory)
{
    return Directory.GetFiles(directory, ScenarioPattern)
        .ToDictionary(file => Path.GetFileName(file), Play, StringComparer.Ordinal);
}

static ulong Play(string file)
{
    var scenario = ScenarioParser.Parse(File.ReadAllText(file));
    return ScenarioRunner.Run(scenario, DefaultRules.Create()).StateHash;
}

static int Usage()
{
    Console.Error.WriteLine("Usage: Rts.Headless run <file.scenario> | print <directory> | verify <directory>");
    return ExitInvalidInput;
}
