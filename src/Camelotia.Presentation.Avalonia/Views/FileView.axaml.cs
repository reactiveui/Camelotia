// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.ObservableEvents;

namespace Camelotia.Presentation.Avalonia.Views;

/// <summary>Shows one listed entry. A double tap opens a folder; the context menu acts on the entry.</summary>
[System.Diagnostics.DebuggerDisplay("FileView")]
public sealed partial class FileView : ReactiveUserControl<IFileViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="FileView"/> class.</summary>
    public FileView()
    {
        InitializeComponent();
        _ = this.WhenActivated(disposables =>
        {
            _ = this.Events()
                .DoubleTapped
                .Where(_ => ViewModel is not null)
                .Do(_ => ViewModel!.Provider.SelectedFile = ViewModel)
                .Select(static _ => RxVoid.Default)
                .InvokeCommand(this, static x => x.ViewModel!.Provider.Open)
                .DisposeWith(disposables);

            _ = ContextMenu!
                .Events()
                .Opening
                .Where(_ => ViewModel is not null)
                .Subscribe(_ => ViewModel!.Provider.SelectedFile = ViewModel)
                .DisposeWith(disposables);
        });
    }
}
