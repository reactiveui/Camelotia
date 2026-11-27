// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.DesignTime;

/// <summary>Sample sign-in state for the designer.</summary>
[System.Diagnostics.DebuggerDisplay("DesignTimeAuthViewModel: {IsAuthenticated}")]
public sealed class DesignTimeAuthViewModel : ReactiveObject, IAuthViewModel
{
    /// <summary>Backs <see cref="DirectAuth"/>.</summary>
    private readonly DesignTimeDirectAuthViewModel _directAuth = new();

    /// <summary>Backs <see cref="HostAuth"/>.</summary>
    private readonly DesignTimeHostAuthViewModel _hostAuth = new();

    /// <summary>Backs <see cref="OAuth"/>.</summary>
    private readonly DesignTimeOAuthViewModel _oauth = new();

    /// <inheritdoc/>
    public IDirectAuthViewModel DirectAuth => _directAuth;

    /// <inheritdoc/>
    public IHostAuthViewModel HostAuth => _hostAuth;

    /// <inheritdoc/>
    public IOAuthViewModel OAuth => _oauth;

    /// <inheritdoc/>
    public bool SupportsDirectAuth => false;

    /// <inheritdoc/>
    public bool SupportsHostAuth => false;

    /// <inheritdoc/>
    public bool SupportsOAuth => false;

    /// <inheritdoc/>
    public bool IsAuthenticated => true;

    /// <inheritdoc/>
    public bool IsAnonymous => false;

    /// <inheritdoc/>
    public void Dispose()
    {
        _directAuth.Dispose();
        _hostAuth.Dispose();
        _oauth.Dispose();
    }
}
