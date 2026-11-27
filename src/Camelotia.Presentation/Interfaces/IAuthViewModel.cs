// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.ComponentModel;

namespace Camelotia.Presentation.Interfaces;

/// <summary>Tracks whether the user is signed in to one provider, and holds its sign-in forms.</summary>
public interface IAuthViewModel : INotifyPropertyChanged, IDisposable
{
    /// <summary>Gets the user name and password form.</summary>
    IDirectAuthViewModel DirectAuth { get; }

    /// <summary>Gets the host, user name and password form.</summary>
    IHostAuthViewModel HostAuth { get; }

    /// <summary>Gets the browser sign-in form.</summary>
    IOAuthViewModel OAuth { get; }

    /// <summary>Gets a value indicating whether the provider signs in with a user name and password.</summary>
    bool SupportsDirectAuth { get; }

    /// <summary>Gets a value indicating whether the provider signs in to a host.</summary>
    bool SupportsHostAuth { get; }

    /// <summary>Gets a value indicating whether the provider signs in through a browser.</summary>
    bool SupportsOAuth { get; }

    /// <summary>Gets a value indicating whether the user is signed in.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Gets a value indicating whether the user is signed out.</summary>
    bool IsAnonymous { get; }
}
