// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Windows;
using System.Windows.Controls;
using Camelotia.Presentation.Interfaces;

namespace Camelotia.Presentation.Wpf.Views;

/// <summary>Shows the host sign-in form: address, port, user name and password.</summary>
[System.Diagnostics.DebuggerDisplay("HostAuthView")]
public sealed partial class HostAuthView : UserControl, IViewFor<IHostAuthViewModel>
{
    /// <summary>Identifies the <see cref="ViewModel"/> property.</summary>
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(IHostAuthViewModel), typeof(HostAuthView));

    /// <summary>Initializes a new instance of the <see cref="HostAuthView"/> class.</summary>
    public HostAuthView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => ViewModel = DataContext as IHostAuthViewModel;
    }

    /// <inheritdoc/>
    public IHostAuthViewModel? ViewModel
    {
        get => (IHostAuthViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (IHostAuthViewModel?)value;
    }
}
