using System.Collections.Immutable;
using System.Globalization;
using System.Text;

namespace Rts.Simulation.Scenarios;

/// <summary>Outcome of comparing one scenario against its golden hash.</summary>
public enum VerificationStatus
{
    Match,
    Mismatch,
    MissingGolden,
    MissingScenario,
}

/// <summary>Comparison result for one scenario; a hash is null when that side has no entry.</summary>
public sealed record VerificationResult(string Name, VerificationStatus Status, ulong? Expected, ulong? Actual);

/// <summary>
/// The checked-in file of expected end-state hashes (ADR-0013). One <c>name hash</c> pair per line,
/// hashes as 16 lowercase hex digits, sorted by name so diffs stay minimal.
/// </summary>
public static class GoldenHashes
{
    private const string HashFormat = "x16";

    /// <summary>Formats a hash the way it appears in the golden file and in CLI output.</summary>
    public static string FormatHash(ulong hash) => hash.ToString(HashFormat, CultureInfo.InvariantCulture);

    /// <exception cref="FormatException">A line is malformed or a name appears twice.</exception>
    public static ImmutableSortedDictionary<string, ulong> Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var builder = ImmutableSortedDictionary.CreateBuilder<string, ulong>(StringComparer.Ordinal);
        var lines = text.Replace("\r\n", "\n").Split('\n');

        for (var index = 0; index < lines.Length; index++)
        {
            var tokens = lines[index].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (tokens.Length == 0 || tokens[0].StartsWith('#'))
            {
                continue;
            }

            var isValid = tokens.Length == 2
                && ulong.TryParse(tokens[1], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var hash)
                && builder.TryAdd(tokens[0], hash);

            if (!isValid)
            {
                throw new FormatException($"Invalid golden hash file at line {index + 1}: expected unique '<name> <hex hash>'.");
            }
        }

        return builder.ToImmutable();
    }

    public static string Format(IReadOnlyDictionary<string, ulong> hashes)
    {
        ArgumentNullException.ThrowIfNull(hashes);

        var text = new StringBuilder();

        foreach (var (name, hash) in hashes.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            text.Append(name).Append(' ').Append(FormatHash(hash)).Append('\n');
        }

        return text.ToString();
    }

    /// <summary>Compares every scenario that appears on either side, sorted by name.</summary>
    public static ImmutableArray<VerificationResult> Verify(
        IReadOnlyDictionary<string, ulong> expected,
        IReadOnlyDictionary<string, ulong> actual)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);

        return [.. expected.Keys.Union(actual.Keys).Order(StringComparer.Ordinal).Select(name => Compare(name, expected, actual))];
    }

    private static VerificationResult Compare(
        string name,
        IReadOnlyDictionary<string, ulong> expected,
        IReadOnlyDictionary<string, ulong> actual)
    {
        var hasExpected = expected.TryGetValue(name, out var expectedHash);
        var hasActual = actual.TryGetValue(name, out var actualHash);
        ulong? expectedOrNull = hasExpected ? expectedHash : null;
        ulong? actualOrNull = hasActual ? actualHash : null;

        var status = (hasExpected, hasActual) switch
        {
            (false, _) => VerificationStatus.MissingGolden,
            (_, false) => VerificationStatus.MissingScenario,
            _ when expectedHash == actualHash => VerificationStatus.Match,
            _ => VerificationStatus.Mismatch,
        };

        return new VerificationResult(name, status, expectedOrNull, actualOrNull);
    }
}
