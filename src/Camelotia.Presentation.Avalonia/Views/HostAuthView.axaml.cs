// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;
using ReactiveUI.Validation.Extensions;

namespace Camelotia.Presentation.Avalonia.Views;

/// <summary>Shows the host sign-in form with its validation messages.</summary>
[System.Diagnostics.DebuggerDisplay("HostAuthView")]
public sealed partial class HostAuthView : ReactiveUserControl<IHostAuthViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="HostAuthView"/> class.</summary>
    public HostAuthView()
    {
        InitializeComponent();
        _ = this.WhenActivated(disposables =>
        {
            _ = this.BindValidation(ViewModel, static x => x.Address, static x => x.AddressValidation.Text).DisposeWith(disposables);
            _ = this.BindValidation(ViewModel, static x => x.Port, static x => x.PortValidation.Text).DisposeWith(disposables);
            _ = this.BindValidation(ViewModel, static x => x.Username, static x => x.UsernameValidation.Text).DisposeWith(disposables);
            _ = this.BindValidation(ViewModel, static x => x.Password, static x => x.PasswordValidation.Text).DisposeWith(disposables);
            _ = this.BindValidation(ViewModel, static x => x.FormValidation.Text).DisposeWith(disposables);
        });
    }
}
