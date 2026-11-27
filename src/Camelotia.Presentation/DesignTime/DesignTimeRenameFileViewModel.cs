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

/// <summary>A sample rename form, with a failing validation rule, for the designer.</summary>
[System.Diagnostics.DebuggerDisplay("DesignTimeRenameFileViewModel: {OldName}")]
public sealed partial class DesignTimeRenameFileViewModel : ReactiveValidationObject, IRenameFileViewModel
{
    /// <summary>Initializes a new instance of the <see cref="DesignTimeRenameFileViewModel"/> class.</summary>
    public DesignTimeRenameFileViewModel() => this.ValidationRule(static x => x.NewName, static _ => false, "Validation error.");

    /// <inheritdoc/>
    public bool IsLoading => false;

    /// <inheritdoc/>
    [Reactive]
    public partial bool IsVisible { get; set; }

    /// <inheritdoc/>
    public string OldName => "file";

    /// <inheritdoc/>
    [Reactive]
    public partial string NewName { get; set; } = string.Empty;

    /// <inheritdoc/>
    public bool HasErrorMessage => true;

    /// <inheritdoc/>
    public string ErrorMessage => "Error message example.";

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Rename { get; } = DesignTimeCommands.Empty();

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Close { get; } = DesignTimeCommands.Empty();

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Open { get; } = DesignTimeCommands.Empty();

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Rename.Dispose();
            Close.Dispose();
            Open.Dispose();
        }

        base.Dispose(disposing);
    }
}
