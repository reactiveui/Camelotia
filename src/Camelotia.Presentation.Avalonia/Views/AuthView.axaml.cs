// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Avalonia.Controls;
using Avalonia.Layout;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;

namespace Camelotia.Presentation.Avalonia.Views;

/// <summary>Shows the sign-in form the provider supports.</summary>
[System.Diagnostics.DebuggerDisplay("AuthView")]
public sealed partial class AuthView : ReactiveUserControl<IAuthViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="AuthView"/> class.</summary>
    public AuthView()
    {
        InitializeComponent();
        _ = this.WhenActivated(disposables =>
            _ = this.WhenAnyValue(static x => x.ViewModel)
                .Where(static context => context is not null)
                .Select(static context => ResolveControl(context!))
                .BindTo(this, static x => x.Content)
                .DisposeWith(disposables));
    }

    /// <summary>Creates the sign-in form for the first sign-in method the provider supports.</summary>
    /// <param name="context">The sign-in view model.</param>
    /// <returns>The form, or a message when the provider supports no sign-in method.</returns>
    private static Control ResolveControl(IAuthViewModel context)
    {
        if (context.SupportsDirectAuth)
        {
            return new DirectAuthView { DataContext = context.DirectAuth };
        }

        if (context.SupportsOAuth)
        {
            return new OAuthView { DataContext = context.OAuth };
        }

        return context.SupportsHostAuth ? new HostAuthView { DataContext = context.HostAuth } : CreateUnsupportedMessage();
    }

    /// <summary>Creates the message shown when the provider supports no sign-in method.</summary>
    /// <returns>The message control.</returns>
    private static TextBlock CreateUnsupportedMessage() =>
        new() { Text = "No supported authentication method found.", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
}
