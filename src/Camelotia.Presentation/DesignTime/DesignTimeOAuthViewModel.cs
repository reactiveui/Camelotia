// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Camelotia.Presentation.DesignTime;

/// <summary>A sample browser sign-in form for the designer.</summary>
[System.Diagnostics.DebuggerDisplay("DesignTimeOAuthViewModel: {ErrorMessage}")]
public sealed class DesignTimeOAuthViewModel : ReactiveObject, IOAuthViewModel
{
    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Login { get; } = DesignTimeCommands.Empty();

    /// <inheritdoc/>
    public bool HasErrorMessage => true;

    /// <inheritdoc/>
    public string ErrorMessage => "Error message example.";

    /// <inheritdoc/>
    public bool IsBusy => false;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => Login.Dispose();
}
