// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Windows;
using Camelotia.Presentation.Interfaces;
using MahApps.Metro.Controls;

namespace Camelotia.Presentation.Wpf;

/// <summary>The main window: the provider list on the left and the selected provider on the right.</summary>
[System.Diagnostics.DebuggerDisplay("MainView")]
public sealed partial class MainView : MetroWindow, IViewFor<IMainViewModel>
{
    /// <summary>Identifies the <see cref="ViewModel"/> property.</summary>
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(IMainViewModel), typeof(MainView));

    /// <summary>Initializes a new instance of the <see cref="MainView"/> class.</summary>
    public MainView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => ViewModel = DataContext as IMainViewModel;
    }

    /// <inheritdoc/>
    public IMainViewModel? ViewModel
    {
        get => (IMainViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (IMainViewModel?)value;
    }
}
