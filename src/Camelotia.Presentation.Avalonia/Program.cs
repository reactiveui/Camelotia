// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using Avalonia;
using Camelotia.Presentation.AppState;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives.Advanced;
using Splat;

namespace Camelotia.Presentation.Avalonia;

/// <summary>Starts the Avalonia desktop app.</summary>
public static class Program
{
    /// <summary>Runs the app until the main window closes.</summary>
    /// <param name="args">The command-line arguments.</param>
    [STAThread]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    /// <summary>Configures Avalonia and ReactiveUI. The Avalonia designer calls this too.</summary>
    /// <returns>The configured app builder.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UseReactiveUI(static builder => builder
                .WithSuspensionHost<MainState>()
                .WithExceptionHandler(Witness.Create<Exception>(static exception => LogHost.Default.Error(exception, "An unhandled error reached ReactiveUI."))))
            .UsePlatformDetect()
            .LogToTrace();
}
