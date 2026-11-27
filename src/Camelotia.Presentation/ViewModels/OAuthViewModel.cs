// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Camelotia.Presentation.ViewModels;

/// <summary>Runs a provider's browser sign-in.</summary>
[System.Diagnostics.DebuggerDisplay("OAuthViewModel: {IsBusy}")]
public sealed class OAuthViewModel : ReactiveObject, IOAuthViewModel
{
    /// <summary>Backs <see cref="ErrorMessage"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _errorMessage;

    /// <summary>Backs <see cref="HasErrorMessage"/>.</summary>
    private readonly ObservableAsPropertyHelper<bool> _hasErrorMessage;

    /// <summary>Backs <see cref="IsBusy"/>.</summary>
    private readonly ObservableAsPropertyHelper<bool> _isBusy;

    /// <summary>Initializes a new instance of the <see cref="OAuthViewModel"/> class.</summary>
    /// <param name="provider">The provider to sign in to.</param>
    public OAuthViewModel(ICloud provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        Login = ReactiveCommand.CreateFromTask(cancellationToken => provider.OAuth(cancellationToken).AsTask());

        _isBusy = Login.IsExecuting.ToProperty(this, static x => x.IsBusy);

        _errorMessage = Login
            .ThrownExceptions
            .Select(static exception => exception.Message)
            .Log(this, $"OAuth error occured in {provider.Name}")
            .ToProperty(this, static x => x.ErrorMessage, string.Empty);

        _hasErrorMessage = Login
            .ThrownExceptions
            .Select(static _ => true)
            .Merge(Login.Select(static _ => false))
            .ToProperty(this, static x => x.HasErrorMessage);
    }

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Login { get; }

    /// <inheritdoc/>
    public string ErrorMessage => _errorMessage.Value;

    /// <inheritdoc/>
    public bool HasErrorMessage => _hasErrorMessage.Value;

    /// <inheritdoc/>
    public bool IsBusy => _isBusy.Value;

    /// <inheritdoc/>
    public void Dispose()
    {
        _errorMessage.Dispose();
        _hasErrorMessage.Dispose();
        _isBusy.Dispose();
        Login.Dispose();
    }
}
