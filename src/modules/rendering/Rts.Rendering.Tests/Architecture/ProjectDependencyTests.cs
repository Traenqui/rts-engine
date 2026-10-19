using System.Xml.Linq;

namespace Rts.Rendering.Tests.Architecture;

/// <summary>
/// Enforces the module boundaries of ADR-0002, ADR-0007 and ADR-0012 by reading the project files,
/// so transitive references and hosts are covered as well.
/// </summary>
public sealed class ProjectDependencyTests
{
    private const string RaylibPackage = "Raylib-cs";
    private const string SolutionFile = "Rts.slnx";

    private static readonly Lazy<IReadOnlyDictionary<string, ProjectNode>> Projects = new(LoadProjects);

    [Theory]
    [InlineData("Rts.Primitives")]
    [InlineData("Rts.Navigation")]
    [InlineData("Rts.Simulation")]
    [InlineData("Rts.Simulation.Contracts")]
    [InlineData("Rts.Rendering.Contracts")]
    [InlineData("Rts.Headless")]
    public void Project_DoesNotDependOnRaylib(string project)
    {
        var packages = Dependencies(project).Append(project).SelectMany(name => Projects.Value[name].Packages);

        Assert.DoesNotContain(RaylibPackage, packages);
    }

    [Theory]
    [InlineData("Rts.Rendering")]
    [InlineData("Rts.Client")]
    public void Project_SeesSimulationOnlyThroughContracts(string project)
    {
        Assert.DoesNotContain("Rts.Simulation", Dependencies(project));
    }

    [Fact]
    public void Projects_AreFoundInRepository()
    {
        Assert.Contains("Rts.Game", Projects.Value.Keys);
        Assert.True(Projects.Value.Count >= 10, $"only {Projects.Value.Count} projects found");
    }

    /// <summary>All projects <paramref name="project"/> references directly or indirectly, excluding itself.</summary>
    private static HashSet<string> Dependencies(string project)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>(Projects.Value[project].ProjectReferences);

        while (pending.TryPop(out var name))
        {
            if (visited.Add(name))
            {
                foreach (var reference in Projects.Value[name].ProjectReferences)
                {
                    pending.Push(reference);
                }
            }
        }

        return visited;
    }

    private static IReadOnlyDictionary<string, ProjectNode> LoadProjects()
    {
        var root = FindRepositoryRoot();

        return Directory.EnumerateFiles(Path.Combine(root, "src"), "*.csproj", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(ReadProject)
            .ToDictionary(node => node.Name, StringComparer.Ordinal);
    }

    private static ProjectNode ReadProject(string path)
    {
        var document = XDocument.Load(path);
        var directory = Path.GetDirectoryName(path)!;

        var projectReferences = document.Descendants("ProjectReference")
            .Select(element => (string)element.Attribute("Include")!)
            .Select(include => Path.GetFileNameWithoutExtension(Path.GetFullPath(Path.Combine(directory, include.Replace('\\', Path.DirectorySeparatorChar)))))
            .ToList();

        var packages = document.Descendants("PackageReference")
            .Select(element => (string)element.Attribute("Include")!)
            .ToList();

        return new ProjectNode(Path.GetFileNameWithoutExtension(path), projectReferences, packages);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, SolutionFile)))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException($"{SolutionFile} not found above {AppContext.BaseDirectory}.");
    }

    private sealed record ProjectNode(string Name, IReadOnlyList<string> ProjectReferences, IReadOnlyList<string> Packages);
}
