// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Windows;
using System.Windows.Controls;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Camelotia.Presentation.Wpf.Views;

/// <summary>Shows one breadcrumb folder, with a chevron that points down while its subfolder menu is open.</summary>
[System.Diagnostics.DebuggerDisplay("FolderView")]
public sealed partial class FolderView : UserControl, IViewFor<IFolderViewModel>
{
    /// <summary>Identifies the <see cref="ViewModel"/> property.</summary>
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(IFolderViewModel), typeof(FolderView));

    /// <summary>Initializes a new instance of the <see cref="FolderView"/> class.</summary>
    public FolderView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => ViewModel = DataContext as IFolderViewModel;
        _ = this.WhenActivated(disposables =>
        {
            _ = this.WhenAnyValue(static x => x.TopLevelMenu.IsSubmenuOpen)
                .Select(static open => open ? Visibility.Collapsed : Visibility.Visible)
                .BindTo(this, static x => x.ChevronRightIcon.Visibility)
                .DisposeWith(disposables);

            _ = this.WhenAnyValue(static x => x.TopLevelMenu.IsSubmenuOpen)
                .Select(static open => open ? Visibility.Visible : Visibility.Collapsed)
                .BindTo(this, static x => x.ChevronDownIcon.Visibility)
                .DisposeWith(disposables);
        });
    }

    /// <inheritdoc/>
    public IFolderViewModel? ViewModel
    {
        get => (IFolderViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (IFolderViewModel?)value;
    }
}
