using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

// ReSharper disable ArrangeTypeMemberModifiers

////[CheckBuildProjectConfigurations(TimeoutInMilliseconds = 2000)]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    const string InteractiveProjectName = "Camelotia.Presentation.Avalonia";
    const string CoverageFileName = "coverage.cobertura.xml";

    public static int Main() => Execute<Build>(x => x.RunInteractive);

    [Parameter] readonly string Configuration = "Release";
    [Parameter] readonly bool Interactive;
    [Parameter] readonly bool Full;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Target Clean => _ => _
        .Before(RunUnitTests)
        .Executes(() => SourceDirectory
            .GlobDirectories("**/bin", "**/obj", "**/AppPackages", "**/BundleArtifacts")
            .Concat(RootDirectory.GlobDirectories("**/artifacts"))
            .ForEach(p => p.DeleteDirectory()));

    Target RunUnitTests => _ => _
        .DependsOn(Clean)
        .Executes(() => SourceDirectory
            .GlobFiles("**/*.Tests.csproj")
            .ForEach(path =>
                DotNetTest(settings => settings
                    .SetProjectFile(path)
                    .SetConfiguration(Configuration)
                    .AddLoggers($"trx;LogFileName={ArtifactsDirectory / "report.trx"}")
                    .AddProperty("CollectCoverage", true)
                    .AddProperty("CoverletOutputFormat", "cobertura")
                    .AddProperty("Exclude", "[xunit.*]*")
                    .AddProperty("CoverletOutput", ArtifactsDirectory / CoverageFileName))));

    Target CompileAvaloniaApp => _ => _
        .DependsOn(RunUnitTests)
        .Executes(() => SourceDirectory
            .GlobFiles("**/*.Avalonia.csproj")
            .ForEach(path =>
                DotNetBuild(settings => settings
                    .SetProjectFile(path)
                    .SetConfiguration(Configuration))));

    Target CompileUniversalWindowsApp => _ => _
        .DependsOn(RunUnitTests)
        .Executes(() =>
        {
            var execute = EnvironmentInfo.IsWin && Full;
            Serilog.Log.Information($"Should compile for Universal Windows: {execute}");
            if (!execute) return;

            Serilog.Log.Information("Restoring packages required by UAP...");
            var project = SourceDirectory.GlobFiles("**/*.Uwp.csproj").First();
            MSBuild(settings => settings
                .SetProjectFile(project)
                .SetTargets("Restore"));
            Serilog.Log.Information("Successfully restored UAP packages.");

            new[] { MSBuildTargetPlatform.x86,
                    MSBuildTargetPlatform.x64,
                    MSBuildTargetPlatform.arm }
                .ForEach(BuildApp);

            void BuildApp(MSBuildTargetPlatform platform)
            {
                Serilog.Log.Information("Cleaning UAP project...");
                MSBuild(settings => settings
                    .SetProjectFile(project)
                    .SetTargets("Clean"));
                Serilog.Log.Information("Successfully managed to clean UAP project.");

                Serilog.Log.Information($"Building UAP project for {platform}...");
                MSBuild(settings => settings
                    .SetProjectFile(project)
                    .SetTargets("Build")
                    .SetConfiguration(Configuration)
                    .SetTargetPlatform(platform)
                    .SetProperty("AppxPackageSigningEnabled", false)
                    .SetProperty("AppxPackageDir", ArtifactsDirectory)
                    .SetProperty("UapAppxPackageBuildMode", "CI")
                    .SetProperty("AppxBundle", "Always"));
                Serilog.Log.Information($"Successfully built UAP project for {platform}.");
            }
        });

    Target CompileXamarinAndroidApp => _ => _
        .DependsOn(RunUnitTests)
        .Executes(() =>
        {
            var execute = EnvironmentInfo.IsWin && Full;
            Serilog.Log.Information($"Should compile for Android: {execute}");
            if (!execute) return;

            Serilog.Log.Information("Restoring packages required by Xamarin Android...");
            var project = SourceDirectory.GlobFiles("**/*.Xamarin.Droid.csproj").First();
            MSBuild(settings => settings
                .SetProjectFile(project)
                .SetTargets("Restore"));
            Serilog.Log.Information("Successfully restored Xamarin Android packages.");

            Serilog.Log.Information("Building Xamarin Android project...");
            var java = Environment.GetEnvironmentVariable("JAVA_HOME");
            MSBuild(settings => settings
                .SetProjectFile(project)
                .SetTargets("Build")
                .SetConfiguration(Configuration)
                .SetProperty("JavaSdkDirectory", java));
            Serilog.Log.Information("Successfully built Xamarin Android project.");

            Serilog.Log.Information("Signing Android package...");
            MSBuild(settings => settings
                .SetProjectFile(project)
                .SetTargets("SignAndroidPackage")
                .SetConfiguration(Configuration)
                .SetProperty("JavaSdkDirectory", java));
            Serilog.Log.Information("Successfully signed Xamarin Android APK.");

            Serilog.Log.Information("Moving APK files to artifacts directory...");
            SourceDirectory
                .GlobFiles("**/bin/**/*-Signed.apk")
                .ForEach(file => MoveFileToDirectory(file, ArtifactsDirectory));
            Serilog.Log.Information("Successfully moved APK files.");
        });

    Target CompileWindowsPresentationApp => _ => _
        .DependsOn(RunUnitTests)
        .Executes(() =>
        {
            var execute = EnvironmentInfo.IsWin && Full;
            Serilog.Log.Information($"Should compile for WPF: {execute}");
            if (!execute) return;

            Serilog.Log.Information("Restoring packages required by WPF app...");
            var project = SourceDirectory.GlobFiles("**/*.Wpf.csproj").First();
            MSBuild(settings => settings
                .SetProjectFile(project)
                .SetTargets("Restore"));
            Serilog.Log.Information("Successfully restored Wpf packages.");

            Serilog.Log.Information("Building WPF project...");
            MSBuild(settings => settings
                .SetProjectFile(project)
                .SetTargets("Build")
                .SetConfiguration(Configuration));
            Serilog.Log.Information("Successfully built WPF project.");
        });

    Target RunInteractive => _ => _
        .DependsOn(CompileAvaloniaApp)
        .DependsOn(CompileUniversalWindowsApp)
        .DependsOn(CompileXamarinAndroidApp)
        .DependsOn(CompileWindowsPresentationApp)
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
