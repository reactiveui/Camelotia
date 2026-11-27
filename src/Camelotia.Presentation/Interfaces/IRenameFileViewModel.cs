// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.ComponentModel;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.Validation.Abstractions;

namespace Camelotia.Presentation.Interfaces;

/// <summary>The rename form.</summary>
public interface IRenameFileViewModel : INotifyDataErrorInfo, IValidatableViewModel, IReactiveObject, IDisposable
{
    /// <summary>Gets a value indicating whether the rename is running.</summary>
    bool IsLoading { get; }

    /// <summary>Gets or sets a value indicating whether the form is open.</summary>
    bool IsVisible { get; set; }

    /// <summary>Gets the selected file's current name.</summary>
    string OldName { get; }

    /// <summary>Gets or sets the new name.</summary>
    string NewName { get; set; }

    /// <summary>Gets a value indicating whether the last attempt failed.</summary>
    bool HasErrorMessage { get; }

    /// <summary>Gets the failure message of the last attempt.</summary>
    string ErrorMessage { get; }

    /// <summary>Gets the command that renames the file.</summary>
    ReactiveCommand<RxVoid, RxVoid> Rename { get; }

    /// <summary>Gets the command that closes the form.</summary>
    ReactiveCommand<RxVoid, RxVoid> Close { get; }

    /// <summary>Gets the command that opens the form.</summary>
    ReactiveCommand<RxVoid, RxVoid> Open { get; }
}
