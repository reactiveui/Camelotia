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

/// <summary>A sample create-folder form, with a failing validation rule, for the designer.</summary>
[System.Diagnostics.DebuggerDisplay("DesignTimeCreateFolderViewModel: {Path}")]
public sealed partial class DesignTimeCreateFolderViewModel : ReactiveValidationObject, ICreateFolderViewModel
{
    /// <summary>Initializes a new instance of the <see cref="DesignTimeCreateFolderViewModel"/> class.</summary>
    public DesignTimeCreateFolderViewModel() => this.ValidationRule(static x => x.Name, static _ => false, "Validation error.");

    /// <inheritdoc/>
    public bool IsLoading => false;

    /// <inheritdoc/>
    [Reactive]
    public partial bool IsVisible { get; set; }

    /// <inheritdoc/>
    [Reactive]
    public partial string Name { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string Path => "/home/path";

    /// <inheritdoc/>
    public bool HasErrorMessage => true;

    /// <inheritdoc/>
    public string ErrorMessage => "Error message example.";

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Create { get; } = DesignTimeCommands.Empty();

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Close { get; } = DesignTimeCommands.Empty();

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Open { get; } = DesignTimeCommands.Empty();

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Create.Dispose();
            Close.Dispose();
            Open.Dispose();
        }

        base.Dispose(disposing);
    }
}
