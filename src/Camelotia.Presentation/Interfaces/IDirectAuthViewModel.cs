// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.ComponentModel;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.Validation.Abstractions;

namespace Camelotia.Presentation.Interfaces;

/// <summary>The user name and password sign-in form.</summary>
public interface IDirectAuthViewModel : INotifyDataErrorInfo, IValidatableViewModel, IReactiveObject, IDisposable
{
    /// <summary>Gets or sets the user name.</summary>
    string Username { get; set; }

    /// <summary>Gets or sets the password.</summary>
    string Password { get; set; }

    /// <summary>Gets the command that signs in.</summary>
    ReactiveCommand<RxVoid, RxVoid> Login { get; }

    /// <summary>Gets a value indicating whether the last sign-in failed.</summary>
    bool HasErrorMessage { get; }

    /// <summary>Gets the failure message of the last sign-in.</summary>
    string ErrorMessage { get; }

    /// <summary>Gets a value indicating whether a sign-in is running.</summary>
    bool IsBusy { get; }
}
