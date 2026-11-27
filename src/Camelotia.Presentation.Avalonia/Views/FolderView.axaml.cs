// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Presentation.Interfaces;
using ReactiveUI.Avalonia;

namespace Camelotia.Presentation.Avalonia.Views;

/// <summary>Shows one breadcrumb folder.</summary>
[System.Diagnostics.DebuggerDisplay("FolderView")]
public sealed partial class FolderView : ReactiveUserControl<IFolderViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="FolderView"/> class.</summary>
    public FolderView() => InitializeComponent();
}
