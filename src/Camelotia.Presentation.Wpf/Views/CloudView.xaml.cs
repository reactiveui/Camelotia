// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Windows;
using System.Windows.Controls;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Camelotia.Presentation.Wpf.Views;

/// <summary>Shows one provider: the current folder, its entries and the file commands.</summary>
[System.Diagnostics.DebuggerDisplay("CloudView")]
public sealed partial class CloudView : UserControl, IViewFor<ICloudViewModel>
{
    /// <summary>Identifies the <see cref="ViewModel"/> property.</summary>
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(ICloudViewModel), typeof(CloudView));

    /// <summary>Initializes a new instance of the <see cref="CloudView"/> class.</summary>
    public CloudView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => ViewModel = DataContext as ICloudViewModel;
        _ = this.WhenActivated(disposables =>
        {
            _ = this.OneWayBind(ViewModel, static vm => vm.ShowBreadCrumbs, static view => view.PathTextBlock.Visibility, static show => show ? Visibility.Collapsed : Visibility.Visible)
                .DisposeWith(disposables);

            _ = this.OneWayBind(ViewModel, static vm => vm.ShowBreadCrumbs, static view => view.BreadCrumbsListBox.Visibility, static show => show ? Visibility.Visible : Visibility.Collapsed)
                .DisposeWith(disposables);
        });
    }

    /// <inheritdoc/>
    public ICloudViewModel? ViewModel
    {
        get => (ICloudViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (ICloudViewModel?)value;
    }
}
