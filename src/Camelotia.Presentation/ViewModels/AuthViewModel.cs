// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Camelotia.Presentation.ViewModels;

/// <summary>Tracks whether the user is signed in to one provider, and holds its sign-in forms.</summary>
[System.Diagnostics.DebuggerDisplay("AuthViewModel: {IsAuthenticated}")]
public sealed class AuthViewModel : ReactiveObject, IAuthViewModel
{
    /// <summary>Backs <see cref="IsAuthenticated"/>.</summary>
    private readonly ObservableAsPropertyHelper<bool> _isAuthenticated;

    /// <summary>Backs <see cref="IsAnonymous"/>.</summary>
    private readonly ObservableAsPropertyHelper<bool> _isAnonymous;

    /// <summary>The provider the user signs in to.</summary>
    private readonly ICloud _provider;

    /// <summary>Initializes a new instance of the <see cref="AuthViewModel"/> class.</summary>
    /// <param name="direct">The user name and password form.</param>
    /// <param name="host">The host, user name and password form.</param>
    /// <param name="open">The browser sign-in form.</param>
    /// <param name="provider">The provider the user signs in to.</param>
    public AuthViewModel(IDirectAuthViewModel direct, IHostAuthViewModel host, IOAuthViewModel open, ICloud provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        OAuth = open;
        HostAuth = host;
        DirectAuth = direct;
        _provider = provider;

        _isAuthenticated = _provider
            .IsAuthorized
            .DistinctUntilChanged()
            .Log(this, $"Authentication state changed for {provider.Name}")
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .ToProperty(this, static x => x.IsAuthenticated);

        _isAnonymous = this
            .WhenAnyValue(static x => x.IsAuthenticated)
            .Select(static authenticated => !authenticated)
            .ToProperty(this, static x => x.IsAnonymous);
    }

    /// <inheritdoc/>
    public bool IsAnonymous => _isAnonymous.Value;

    /// <inheritdoc/>
    public bool IsAuthenticated => _isAuthenticated.Value;

    /// <inheritdoc/>
    public bool SupportsDirectAuth => _provider.SupportsDirectAuth;

    /// <inheritdoc/>
    public bool SupportsHostAuth => _provider.SupportsHostAuth;

    /// <inheritdoc/>
    public bool SupportsOAuth => _provider.SupportsOAuth;

    /// <inheritdoc/>
    public IDirectAuthViewModel DirectAuth { get; }

    /// <inheritdoc/>
    public IHostAuthViewModel HostAuth { get; }

    /// <inheritdoc/>
    public IOAuthViewModel OAuth { get; }

    /// <inheritdoc/>
    public void Dispose()
    {
        _isAuthenticated.Dispose();
        _isAnonymous.Dispose();
        DirectAuth.Dispose();
        HostAuth.Dispose();
        OAuth.Dispose();
    }
}
