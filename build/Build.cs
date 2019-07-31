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
    
    [Parameter] public readonly string Configuration = IsLocalBuild ? "Debug" : "Release";
    [Parameter] public readonly bool Interactive;
    [Parameter] public readonly bool Full;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Target Clean => _ => _
        .Before(Test)
        .Executes(() => SourceDirectory
            .GlobDirectories("**/bin", "**/obj", "**/artifacts", "**/AppPackages", "**/BundleArtifacts")
            .ForEach(DeleteDirectory));
    
    Target Test => _ => _
        .DependsOn(Clean)
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
            Logger.Info($"Should compile for Universal Windows: {execute}");
            if (!execute) return;

            Logger.Normal("Restoring packages required by UAP...");
            var project = SourceDirectory.GlobFiles("**/*.Uwp.csproj").First();
            MSBuild(settings => settings
                .SetProjectFile(project)
                .SetTargets("Restore"));
            Logger.Success("Successfully restored UAP packages.");

            new[] { MSBuildTargetPlatform.x64,
                    MSBuildTargetPlatform.x86,
                    MSBuildTargetPlatform.arm }
                .ForEach(BuildApp);

            void BuildApp(MSBuildTargetPlatform platform)
            {
                Logger.Normal($"Building UAP project for {platform}");
                MSBuild(settings => settings
                    .SetProjectFile(project)
                    .SetTargets("Build")
                    .SetConfiguration(Configuration)
                    .SetTargetPlatform(platform)
                    .SetProperty("AppxPackageSigningEnabled", false)
                    .SetProperty("UapAppxPackageBuildMode", "CI")
                    .SetProperty("AppxBundle", "Always"));
                Logger.Success($"Successfully built UAP project for {platform}");
            }
        });

    Target CompileXamarinAndroidApp => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            var execute = EnvironmentInfo.IsWin && Full;
            Logger.Normal($"Should compile for Android: {execute}");
            if (!execute) return;
            
            var project = SourceDirectory.GlobFiles("**/*.Xamarin.Droid.csproj").First();
            MSBuild(settings => settings
                .SetProjectFile(project)
                .SetConfiguration(Configuration)
                .SetTargetPlatform(MSBuildTargetPlatform.x86));
        });

    Target Run => _ => _
        .DependsOn(CompileAvaloniaApp)
        .DependsOn(CompileUniversalWindowsApp)
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
