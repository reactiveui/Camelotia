using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable ArrangeTypeMemberModifiers

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
internal class Build : NukeBuild
{
    private const string CoverageFileName = "coverage.cobertura.xml";
    private static readonly IReadOnlyList<string> CrossPlatformProjects = new List<string>
    {
        "Camelotia.Presentation.Tests.csproj",
        "Camelotia.Presentation.Avalonia.csproj"
    };
    
    private static readonly IReadOnlyList<string> WindowsOnlyProjects = new List<string>
    {
        "Camelotia.Presentation.Xamarin.csproj",
        "Camelotia.Presentation.Uwp.csproj"
    };
    
    public static int Main() => Execute<Build>(x => x.Run);
    
    [Parameter("Compilation configuration, can either be Debug or Release.")] 
    public readonly string Configuration = IsLocalBuild ? "Debug" : "Release";

    [Parameter("If interactive is set to true, then a GUI launches.")]
    public readonly bool Interactive = false;

    AbsolutePath SourceDirectory => RootDirectory / "src";

    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() => SourceDirectory
            .GlobDirectories("**/bin", "**/obj", "**/artifacts")
            .ForEach(DeleteDirectory));

    Target Restore => _ => _
        .Executes(() => SourceDirectory
            .GlobFiles("**/*.csproj")
            .Where(SupportedByEnvironment)
            .ForEach(path =>
                DotNetRestore(s => s
                    .SetProjectFile(path))));

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() => SourceDirectory
            .GlobFiles("**/*.csproj")
            .Where(SupportedByEnvironment)
            .ForEach(path =>
                DotNetBuild(s => s
                    .SetProjectFile(path)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore())));

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() => SourceDirectory
            .GlobFiles("**/*.Tests.csproj")
            .Where(SupportedByEnvironment)
            .ForEach(path => 
                DotNetTest(s => s
                    .SetProjectFile(path)
                    .SetConfiguration(Configuration)
                    .EnableNoBuild()
                    .EnableNoRestore()
                    .SetLogger($"trx;LogFileName={ArtifactsDirectory / "report.trx"}")
                    .AddProperty("CollectCoverage", true)
                    .AddProperty("CoverletOutputFormat", "cobertura")
                    .AddProperty("Exclude", "[xunit.*]*")
                    .AddProperty("CoverletOutput", ArtifactsDirectory / CoverageFileName))));

    Target Run => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            if (!Interactive) return;
            var runnable = SourceDirectory
                .GlobFiles("**/*.Avalonia.csproj")
                .First();

            DotNetRun(s => s
                .SetProjectFile(runnable)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore());
        });

    bool SupportedByEnvironment(AbsolutePath path)
    {
        var projects = CrossPlatformProjects.ToList();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            projects.AddRange(WindowsOnlyProjects);
        
        return projects.Any(name => path.ToString().Contains(name));
    }
}
