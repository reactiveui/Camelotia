// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Windows;
using System.Windows.Controls;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.ObservableEvents;

namespace Camelotia.Presentation.Wpf.Views;

/// <summary>Shows one listed entry. A double click opens a folder.</summary>
[System.Diagnostics.DebuggerDisplay("FileView")]
public sealed partial class FileView : UserControl, IViewFor<IFileViewModel>
{
    /// <summary>Identifies the <see cref="ViewModel"/> property.</summary>
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(IFileViewModel), typeof(FileView));

    /// <summary>Initializes a new instance of the <see cref="FileView"/> class.</summary>
    public FileView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => ViewModel = DataContext as IFileViewModel;
        _ = this.WhenActivated(disposables =>
            _ = this.Events()
                .MouseDoubleClick
                .Where(_ => ViewModel is not null)
                .Do(_ => ViewModel!.Provider.SelectedFile = ViewModel)
                .Select(static _ => RxVoid.Default)
                .InvokeCommand(this, static x => x.ViewModel!.Provider.Open)
                .DisposeWith(disposables));
    }

    /// <inheritdoc/>
    public IFileViewModel? ViewModel
    {
        get => (IFileViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (IFileViewModel?)value;
    }
}
