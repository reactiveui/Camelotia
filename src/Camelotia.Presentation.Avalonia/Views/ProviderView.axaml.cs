// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives.Disposables;

namespace Camelotia.Presentation.Avalonia.Views;

/// <summary>Shows one provider's browser.</summary>
/// <remarks>The view activates, so the browser view model starts and stops its automatic refresh with it.</remarks>
[System.Diagnostics.DebuggerDisplay("ProviderView")]
public sealed partial class ProviderView : ReactiveUserControl<ICloudViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="ProviderView"/> class.</summary>
    public ProviderView()
    {
        InitializeComponent();
        _ = this.WhenActivated(static (MultipleDisposable _) => { });
    }
}
