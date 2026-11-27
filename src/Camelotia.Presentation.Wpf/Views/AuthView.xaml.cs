// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Windows;
using System.Windows.Controls;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Camelotia.Presentation.Wpf.Views;

/// <summary>Shows the sign-in forms and opens the tab for the method the provider supports.</summary>
[System.Diagnostics.DebuggerDisplay("AuthView")]
public sealed partial class AuthView : UserControl, IViewFor<IAuthViewModel>
{
    /// <summary>Identifies the <see cref="ViewModel"/> property.</summary>
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(IAuthViewModel), typeof(AuthView));

    /// <summary>The tab that shows the host sign-in form.</summary>
    private const int HostAuthTab = 1;

    /// <summary>The tab that shows the browser sign-in form.</summary>
    private const int OAuthTab = 2;

    /// <summary>Initializes a new instance of the <see cref="AuthView"/> class.</summary>
    public AuthView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => ViewModel = DataContext as IAuthViewModel;
        _ = this.WhenActivated(disposables =>
        {
            _ = this.WhenAnyValue(static x => x.ViewModel!.SupportsDirectAuth)
                .Where(static supports => supports)
                .Subscribe(_ => AuthTabs.SelectedIndex = 0)
                .DisposeWith(disposables);

            _ = this.WhenAnyValue(static x => x.ViewModel!.SupportsHostAuth)
                .Where(static supports => supports)
                .Subscribe(_ => AuthTabs.SelectedIndex = HostAuthTab)
                .DisposeWith(disposables);

            _ = this.WhenAnyValue(static x => x.ViewModel!.SupportsOAuth)
                .Where(static supports => supports)
                .Subscribe(_ => AuthTabs.SelectedIndex = OAuthTab)
                .DisposeWith(disposables);
        });
    }

    /// <inheritdoc/>
    public IAuthViewModel? ViewModel
    {
        get => (IAuthViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (IAuthViewModel?)value;
    }
}
