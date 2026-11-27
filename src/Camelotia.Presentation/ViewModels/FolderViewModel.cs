// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Models;
using ReactiveUI;

namespace Camelotia.Presentation.ViewModels;

/// <summary>Shows one folder on the breadcrumb trail, with its subfolders.</summary>
[System.Diagnostics.DebuggerDisplay("FolderViewModel: {FullPath}")]
public sealed class FolderViewModel : ReactiveObject, IFolderViewModel
{
    /// <summary>The folder the provider listed.</summary>
    private readonly FolderModel _folder;

    /// <summary>Initializes a new instance of the <see cref="FolderViewModel"/> class.</summary>
    /// <param name="provider">The provider that listed the folder.</param>
    /// <param name="folder">The folder.</param>
    public FolderViewModel(ICloudViewModel provider, FolderModel folder)
    {
        ArgumentNullException.ThrowIfNull(folder);
        Provider = provider;
        _folder = folder;
        var children = new List<IFolderViewModel>();
        foreach (var child in folder.Children)
        {
            children.Add(new FolderViewModel(provider, child));
        }

        Children = children;
    }

    /// <inheritdoc/>
    public ICloudViewModel Provider { get; }

    /// <inheritdoc/>
    public string Name => _folder.Name;

    /// <inheritdoc/>
    public string FullPath => _folder.FullPath;

    /// <inheritdoc/>
    public IEnumerable<IFolderViewModel> Children { get; }

    /// <summary>Checks whether another value is a view of the same folder from the same provider.</summary>
    /// <param name="obj">The value to compare.</param>
    /// <returns><see langword="true"/> when the name, path and provider match.</returns>
    public override bool Equals(object? obj) =>
        obj is FolderViewModel other
        && string.Equals(_folder.Name, other._folder.Name, StringComparison.Ordinal)
        && string.Equals(_folder.FullPath, other._folder.FullPath, StringComparison.Ordinal)
        && Equals(Provider, other.Provider);

    /// <summary>Combines the name, path and provider into a hash code.</summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() =>
        HashCode.Combine(StringComparer.Ordinal.GetHashCode(_folder.Name), StringComparer.Ordinal.GetHashCode(_folder.FullPath), Provider);
}
