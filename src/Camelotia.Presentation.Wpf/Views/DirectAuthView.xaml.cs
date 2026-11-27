// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Windows;
using System.Windows.Controls;
using Camelotia.Presentation.Interfaces;

namespace Camelotia.Presentation.Wpf.Views;

/// <summary>Shows the user name and password sign-in form.</summary>
[System.Diagnostics.DebuggerDisplay("DirectAuthView")]
public sealed partial class DirectAuthView : UserControl, IViewFor<IDirectAuthViewModel>
{
    /// <summary>Identifies the <see cref="ViewModel"/> property.</summary>
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(IDirectAuthViewModel), typeof(DirectAuthView));

    /// <summary>Initializes a new instance of the <see cref="DirectAuthView"/> class.</summary>
    public DirectAuthView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => ViewModel = DataContext as IDirectAuthViewModel;
    }

    /// <inheritdoc/>
    public IDirectAuthViewModel? ViewModel
    {
        get => (IDirectAuthViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (IDirectAuthViewModel?)value;
    }
}
