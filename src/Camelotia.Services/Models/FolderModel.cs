// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
namespace Camelotia.Services.Models;

/// <summary>A folder on a breadcrumb trail, with its subfolders.</summary>
[System.Diagnostics.DebuggerDisplay("FolderModel: {FullPath}")]
public class FolderModel
{
    /// <summary>Initializes a new instance of the <see cref="FolderModel"/> class with no subfolders.</summary>
    /// <param name="fullPath">The full path of the folder.</param>
    /// <param name="name">The display name.</param>
    public FolderModel(string fullPath, string name)
        : this(fullPath, name, [])
    {
    }

    /// <summary>Initializes a new instance of the <see cref="FolderModel"/> class.</summary>
    /// <param name="fullPath">The full path of the folder.</param>
    /// <param name="name">The display name.</param>
    /// <param name="children">The subfolders.</param>
    public FolderModel(string fullPath, string name, IEnumerable<FolderModel> children)
    {
        FullPath = fullPath;
        Name = name;
        Children = children;
    }

    /// <summary>Gets the full path of the folder.</summary>
    public string FullPath { get; }

    /// <summary>Gets the display name.</summary>
    public string Name { get; }

    /// <summary>Gets the subfolders.</summary>
    public IEnumerable<FolderModel> Children { get; }
}
