﻿using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Camelotia.Presentation.Uwp.Views;

public sealed partial class AuthView : UserControl, IViewFor<IAuthViewModel>
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty
        .Register(nameof(ViewModel), typeof(IAuthViewModel), typeof(AuthView), null);

    public AuthView()
    {
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(x => x.ViewModel.SupportsDirectAuth)
                .Where(supports => supports)
                .Subscribe(supports => AuthorizationPivot.SelectedIndex = 0)
                .DisposeWith(disposables);

            this.WhenAnyValue(x => x.ViewModel.SupportsOAuth)
                .Where(supports => supports)
                .Subscribe(supports => AuthorizationPivot.SelectedIndex = 1)
                .DisposeWith(disposables);

            this.WhenAnyValue(x => x.ViewModel.SupportsHostAuth)
                .Where(supports => supports)
                .Subscribe(supports => AuthorizationPivot.SelectedIndex = 2)
                .DisposeWith(disposables);

            this.WhenAnyValue(
                    x => x.ViewModel.SupportsDirectAuth,
                    x => x.ViewModel.SupportsHostAuth,
                    x => x.ViewModel.SupportsOAuth)
                .Subscribe(x => AuthorizationPivot.Visibility = Visibility.Visible)
                .DisposeWith(disposables);
        });
    }

    public IAuthViewModel ViewModel
    {
        get => (IAuthViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (IAuthViewModel)value;
    }
}
