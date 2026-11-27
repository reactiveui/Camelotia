// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Globalization;
using Camelotia.Presentation.Extensions;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Models;
using ReactiveUI;

namespace Camelotia.Presentation.ViewModels;

/// <summary>Shows one file or folder in a provider's listing.</summary>
[System.Diagnostics.DebuggerDisplay("FileViewModel: {Path}")]
public sealed class FileViewModel : ReactiveObject, IFileViewModel
{
    /// <summary>The entry the provider listed.</summary>
    private readonly FileModel _file;

    /// <summary>Initializes a new instance of the <see cref="FileViewModel"/> class.</summary>
    /// <param name="provider">The provider that listed the entry.</param>
    /// <param name="file">The entry.</param>
    public FileViewModel(ICloudViewModel provider, FileModel file)
    {
        Provider = provider;
        _file = file;
    }

    /// <inheritdoc/>
    public ICloudViewModel Provider { get; }

    /// <inheritdoc/>
    public string Modified => _file.Modified?.ToString(CultureInfo.InvariantCulture) ?? "A long time ago";

    /// <inheritdoc/>
    public string Size => _file.Size == 0 ? "Unknown" : _file.Size.ByteSizeToString();

    /// <inheritdoc/>
    public bool IsFolder => _file.IsFolder;

    /// <inheritdoc/>
    public bool IsFile => !_file.IsFolder;

    /// <inheritdoc/>
    public string Name => _file.Name;

    /// <inheritdoc/>
    public string Path => _file.Path;

    /// <summary>Checks whether another value is a view of the same entry from the same provider.</summary>
    /// <param name="obj">The value to compare.</param>
    /// <returns><see langword="true"/> when the path, change time and provider match.</returns>
    public override bool Equals(object? obj) =>
        obj is FileViewModel other
        && string.Equals(_file.Path, other._file.Path, StringComparison.Ordinal)
        && _file.Modified == other._file.Modified
        && Equals(Provider, other.Provider);

    /// <summary>Combines the path, change time and provider into a hash code.</summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(StringComparer.Ordinal.GetHashCode(_file.Path), _file.Modified, Provider);
}
