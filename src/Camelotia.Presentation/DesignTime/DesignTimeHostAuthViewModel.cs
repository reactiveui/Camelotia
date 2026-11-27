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

/// <summary>A sample host sign-in form, with a failing validation rule, for the designer.</summary>
[System.Diagnostics.DebuggerDisplay("DesignTimeHostAuthViewModel: {Address}:{Port}")]
public sealed partial class DesignTimeHostAuthViewModel : ReactiveValidationObject, IHostAuthViewModel
{
    /// <summary>Initializes a new instance of the <see cref="DesignTimeHostAuthViewModel"/> class.</summary>
    public DesignTimeHostAuthViewModel() => this.ValidationRule(static x => x.Username, static _ => false, "Validation error.");

    /// <inheritdoc/>
    [Reactive]
    public partial string Username { get; set; } = "Jotaro";

    /// <inheritdoc/>
    [Reactive]
    public partial string Password { get; set; } = "Qwerty";

    /// <inheritdoc/>
    [Reactive]
    public partial string Address { get; set; } = "127.0.0.1";

    /// <inheritdoc/>
    [Reactive]
    public partial string Port { get; set; } = "5001";

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
