// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.SourceGenerators;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace Camelotia.Presentation.DesignTime;

/// <summary>A sample user name and password form, with a failing validation rule, for the designer.</summary>
[System.Diagnostics.DebuggerDisplay("DesignTimeDirectAuthViewModel: {Username}")]
public sealed partial class DesignTimeDirectAuthViewModel : ReactiveValidationObject, IDirectAuthViewModel
{
    /// <summary>Initializes a new instance of the <see cref="DesignTimeDirectAuthViewModel"/> class.</summary>
    public DesignTimeDirectAuthViewModel() => this.ValidationRule(static x => x.Username, static _ => false, "Validation error.");

    /// <inheritdoc/>
    [Reactive]
    public partial string Username { get; set; } = "Joseph";

    /// <inheritdoc/>
    [Reactive]
    public partial string Password { get; set; } = string.Empty;

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Login { get; } = DesignTimeCommands.Empty();

    /// <inheritdoc/>
    public bool HasErrorMessage => true;

    /// <inheritdoc/>
    public string ErrorMessage => "Error message example.";

    /// <inheritdoc/>
    public bool IsBusy => false;

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Login.Dispose();
        }

        base.Dispose(disposing);
    }
}
