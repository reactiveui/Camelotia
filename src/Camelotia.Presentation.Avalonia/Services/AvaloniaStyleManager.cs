// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Styling;

namespace Camelotia.Presentation.Avalonia.Services;

/// <summary>Switches the app between the light and dark Fluent theme variants.</summary>
/// <param name="application">The application whose theme variant changes.</param>
[System.Diagnostics.DebuggerDisplay("AvaloniaStyleManager: {CurrentTheme}")]
public sealed class AvaloniaStyleManager(Application application)
{
    /// <summary>Gets the theme variant in use.</summary>
    public ThemeVariant CurrentTheme => application.ActualThemeVariant;

    /// <summary>Switches to the other theme variant.</summary>
    public void UseNextTheme() =>
        application.RequestedThemeVariant = CurrentTheme == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
}
