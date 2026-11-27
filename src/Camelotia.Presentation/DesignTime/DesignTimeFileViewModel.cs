// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.DesignTime;

/// <summary>A sample listed file for the designer.</summary>
/// <param name="provider">The provider view model the file belongs to.</param>
[System.Diagnostics.DebuggerDisplay("DesignTimeFileViewModel: {Name}")]
public sealed class DesignTimeFileViewModel(ICloudViewModel provider) : ReactiveObject, IFileViewModel
{
    /// <summary>Initializes a new instance of the <see cref="DesignTimeFileViewModel"/> class that belongs to the shared sample provider.</summary>
    public DesignTimeFileViewModel()
        : this(DesignTimeCloudViewModel.Shared)
    {
    }

    /// <inheritdoc/>
    public string Name => "Awesome file.";

    /// <inheritdoc/>
    public ICloudViewModel Provider { get; } = provider;

    /// <inheritdoc/>
    public string Modified => "2019-01-01 00:00";

    /// <inheritdoc/>
    public bool IsFolder => false;

    /// <inheritdoc/>
    public bool IsFile => true;

    /// <inheritdoc/>
    public string Path => "/home/path/file";

    /// <inheritdoc/>
    public string Size => "42 KB";
}
