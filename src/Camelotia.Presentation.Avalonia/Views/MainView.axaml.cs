// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Presentation.Interfaces;
using ReactiveUI.Avalonia;

namespace Camelotia.Presentation.Avalonia.Views;

/// <summary>Shows the provider list and the selected provider's browser.</summary>
[System.Diagnostics.DebuggerDisplay("MainView")]
public sealed partial class MainView : ReactiveUserControl<IMainViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="MainView"/> class.</summary>
    public MainView() => InitializeComponent();
}
