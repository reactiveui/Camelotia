// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Windows;
using System.Windows.Controls;
using Camelotia.Presentation.Interfaces;

namespace Camelotia.Presentation.Wpf.Views;

/// <summary>Shows the browser sign-in form.</summary>
[System.Diagnostics.DebuggerDisplay("OAuthView")]
public sealed partial class OAuthView : UserControl, IViewFor<IOAuthViewModel>
{
    /// <summary>Identifies the <see cref="ViewModel"/> property.</summary>
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(IOAuthViewModel), typeof(OAuthView));

    /// <summary>Initializes a new instance of the <see cref="OAuthView"/> class.</summary>
    public OAuthView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => ViewModel = DataContext as IOAuthViewModel;
    }

    /// <inheritdoc/>
    public IOAuthViewModel? ViewModel
    {
        get => (IOAuthViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (IOAuthViewModel?)value;
    }
}
