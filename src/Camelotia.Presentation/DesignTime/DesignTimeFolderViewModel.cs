// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.DesignTime;

/// <summary>A sample breadcrumb folder for the designer.</summary>
[System.Diagnostics.DebuggerDisplay("DesignTimeFolderViewModel: {Name}")]
public sealed class DesignTimeFolderViewModel : ReactiveObject, IFolderViewModel
{
    /// <summary>Initializes a new instance of the <see cref="DesignTimeFolderViewModel"/> class with three sample subfolders.</summary>
    public DesignTimeFolderViewModel()
        : this("home", [new DesignTimeFolderViewModel("home"), new DesignTimeFolderViewModel("home1"), new DesignTimeFolderViewModel("home2")])
    {
    }

    /// <summary>Initializes a new instance of the <see cref="DesignTimeFolderViewModel"/> class with no subfolders.</summary>
    /// <param name="name">The folder name.</param>
    public DesignTimeFolderViewModel(string name)
        : this(name, [])
    {
    }

    /// <summary>Initializes a new instance of the <see cref="DesignTimeFolderViewModel"/> class.</summary>
    /// <param name="name">The folder name.</param>
    /// <param name="children">The subfolders.</param>
    public DesignTimeFolderViewModel(string name, IEnumerable<IFolderViewModel> children)
    {
        Name = name;
        FullPath = $"/{name}";
        Children = children;
    }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public string FullPath { get; }

    /// <inheritdoc/>
    public IEnumerable<IFolderViewModel> Children { get; }

    /// <inheritdoc/>
    public ICloudViewModel Provider => DesignTimeCloudViewModel.Shared;
}
