// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Windows;
using System.Windows.Controls;
using Camelotia.Presentation.Interfaces;

namespace Camelotia.Presentation.Wpf.Views;

/// <summary>Shows the form that renames the selected file.</summary>
[System.Diagnostics.DebuggerDisplay("RenameFileView")]
public sealed partial class RenameFileView : UserControl, IViewFor<IRenameFileViewModel>
{
    /// <summary>Identifies the <see cref="ViewModel"/> property.</summary>
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(IRenameFileViewModel), typeof(RenameFileView));

    /// <summary>Initializes a new instance of the <see cref="RenameFileView"/> class.</summary>
    public RenameFileView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => ViewModel = DataContext as IRenameFileViewModel;
    }

    /// <inheritdoc/>
    public IRenameFileViewModel? ViewModel
    {
        get => (IRenameFileViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (IRenameFileViewModel?)value;
    }
}
