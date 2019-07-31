using System.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

// ReSharper disable ArrangeTypeMemberModifiers

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
internal class Build : NukeBuild
{
    private const string InteractiveProjectName = "Camelotia.Presentation.Avalonia";
    private const string CoverageFileName = "coverage.cobertura.xml";

    public static int Main() => Execute<Build>(x => x.Run);
    
    [Parameter("Compilation configuration, can either be Debug or Release.")] 
    public readonly string Configuration = IsLocalBuild ? "Debug" : "Release";
    
    [Parameter("If interactive is set to true, a GUI launches.")]
    public readonly bool Interactive;
    
    [Parameter("If full is set to true, then all projects will build.")]
    public readonly bool Full;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Target Clean => _ => _
        .Before(Test)
        .Executes(() => SourceDirectory
            .GlobDirectories("**/bin", "**/obj", "**/artifacts")
            .ForEach(DeleteDirectory));
    
    Target Test => _ => _
        .Executes(() => SourceDirectory
            .GlobFiles("**/*.Tests.csproj")
            .ForEach(path =>
                DotNetTest(settings => settings
                    .SetProjectFile(path)
                    .SetConfiguration(Configuration)
                    .SetLogger($"trx;LogFileName={ArtifactsDirectory / "report.trx"}")
                    .AddProperty("CollectCoverage", true)
                    .AddProperty("CoverletOutputFormat", "cobertura")
                    .AddProperty("Exclude", "[xunit.*]*")
                    .AddProperty("CoverletOutput", ArtifactsDirectory / CoverageFileName))));

    Target CompileAvaloniaApp => _ => _
        .DependsOn(Test)
        .Executes(() => SourceDirectory
            .GlobFiles("**/*.Avalonia.csproj")
            .ForEach(path =>
                DotNetBuild(settings => settings
                    .SetProjectFile(path)
                    .SetConfiguration(Configuration))));

    private Target CompileUniversalWindowsApp => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            var execute = EnvironmentInfo.IsWin && Full;
            Logger.Normal($"Should compile for Universal Windows: {execute}");
            if (!execute) return;
            
            var project = SourceDirectory.GlobFiles("**/*.Uwp.csproj").First();
            MSBuild(settings => settings
                .SetProjectFile(project)
                .SetConfiguration(Configuration)
                .SetTargetPlatform(MSBuildTargetPlatform.x86));
        });

    Target CompileXamarinAndroidApp => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            var execute = EnvironmentInfo.IsWin && Full;
            Logger.Normal($"Should compile for Android: {execute}");
            if (!execute) return;
            
            var project = SourceDirectory.GlobFiles("**/*.Xamarin.Android.csproj").First();
            MSBuild(settings => settings
                .SetProjectFile(project)
                .SetConfiguration(Configuration)
                .SetTargetPlatform(MSBuildTargetPlatform.x86));
        });

    Target Run => _ => _
        .DependsOn(CompileAvaloniaApp)
        .DependsOn(CompileUniversalWindowsApp)
        .DependsOn(CompileXamarinAndroidApp)
        .Executes(() => SourceDirectory
            .GlobFiles($"**/{InteractiveProjectName}.csproj")
            .Where(x => Interactive)
            .ForEach(path => 
                DotNetRun(settings => settings
                    .SetProjectFile(path)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore()
                    .EnableNoBuild())));
}
