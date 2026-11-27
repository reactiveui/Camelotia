// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
namespace Camelotia.Presentation.Wpf;

/// <summary>Starts the WPF desktop app.</summary>
public static class Program
{
    /// <summary>Runs the app until the main window closes.</summary>
    /// <returns>The app's exit code.</returns>
    [STAThread]
    public static int Main()
    {
        using var app = new App();
        return app.Run();
    }
}
