using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
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
    const string InteractiveProjectName = "Camelotia.Presentation.Avalonia";
    const string CoverageFileName = "coverage.cobertura.xml";

    public static int Main() => Execute<Build>(x => x.RunInteractive);

    [Parameter] readonly string Configuration = IsLocalBuild ? "Debug" : "Release";
    [Parameter] readonly bool Interactive;
    [Parameter] readonly bool Full;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Target Clean => _ => _
        .Before(RunUnitTests)
        .Executes(() => SourceDirectory
            .GlobDirectories("**/bin", "**/obj", "**/AppPackages", "**/BundleArtifacts")
            .Concat(RootDirectory.GlobDirectories("**/artifacts"))
            .ForEach(DeleteDirectory));

    Target RunUnitTests => _ => _
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
            Logger.Info($"Should compile for Universal Windows: {execute}");
            if (!execute) return;

            Logger.Normal("Restoring packages required by UAP...");
            var project = SourceDirectory.GlobFiles("**/*.Uwp.csproj").First();
            MSBuild(settings => settings
                .SetProjectFile(project)
                .SetTargets("Restore"));
            Logger.Success("Successfully restored UAP packages.");

            new[] { MSBuildTargetPlatform.x86,
                    MSBuildTargetPlatform.x64,
                    MSBuildTargetPlatform.arm }
                .ForEach(BuildApp);

            void BuildApp(MSBuildTargetPlatform platform)
            {
                Logger.Normal("Cleaning UAP project...");
                MSBuild(settings => settings
                    .SetProjectFile(project)
                    .SetTargets("Clean"));
                Logger.Success("Successfully managed to clean UAP project.");

                Logger.Normal($"Building UAP project for {platform}...");
                MSBuild(settings => settings
                    .SetProjectFile(project)
                    .SetTargets("Build")
                    .SetConfiguration(Configuration)
                    .SetTargetPlatform(platform)
                    .SetProperty("AppxPackageSigningEnabled", false)
                    .SetProperty("AppxPackageDir", ArtifactsDirectory)
                    .SetProperty("UapAppxPackageBuildMode", "CI")
                    .SetProperty("AppxBundle", "Always"));
                Logger.Success($"Successfully built UAP project for {platform}.");
            }
        });

    Target CompileXamarinAndroidApp => _ => _
        .DependsOn(RunUnitTests)
        .Executes(() =>
        {
            var execute = EnvironmentInfo.IsWin && Full;
            Logger.Info($"Should compile for Android: {execute}");
            if (!execute) return;

            Logger.Normal("Restoring packages required by Xamarin Android...");
            var project = SourceDirectory.GlobFiles("**/*.Xamarin.Droid.csproj").First();
            MSBuild(settings => settings
                .SetProjectFile(project)
                .SetTargets("Restore"));
            Logger.Success("Successfully restored Xamarin Android packages.");

            Logger.Normal("Building Xamarin Android project...");
            var java = Environment.GetEnvironmentVariable("JAVA_HOME");
            MSBuild(settings => settings
                .SetProjectFile(project)
                .SetTargets("Build")
                .SetConfiguration(Configuration)
                .SetProperty("JavaSdkDirectory", java));
            Logger.Success("Successfully built Xamarin Android project.");

            Logger.Normal("Signing Android package...");
            MSBuild(settings => settings
                .SetProjectFile(project)
                .SetTargets("SignAndroidPackage")
                .SetConfiguration(Configuration)
                .SetProperty("JavaSdkDirectory", java));
            Logger.Success("Successfully signed Xamarin Android APK.");

            Logger.Normal("Moving APK files to artifacts directory...");
            SourceDirectory
                .GlobFiles("**/bin/**/*-Signed.apk")
                .ForEach(file => MoveFileToDirectory(file, ArtifactsDirectory));
            Logger.Success("Successfully moved APK files.");
        });

    Target CompileWindowsPresentationApp => _ => _
        .DependsOn(RunUnitTests)
        .Executes(() =>
        {
            var execute = EnvironmentInfo.IsWin && Full;
            Logger.Info($"Should compile for WPF: {execute}");
            if (!execute) return;

            Logger.Normal("Restoring packages required by WPF app...");
            var project = SourceDirectory.GlobFiles("**/*.Wpf.csproj").First();
            MSBuild(settings => settings
                .SetProjectFile(project)
                .SetTargets("Restore"));
            Logger.Success("Successfully restored Wpf packages.");

            Logger.Normal("Building WPF project...");
            MSBuild(settings => settings
                .SetProjectFile(project)
                .SetTargets("Build")
                .SetConfiguration(Configuration));
            Logger.Success("Successfully built WPF project.");
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
