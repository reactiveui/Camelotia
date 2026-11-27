// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.ComponentModel;

namespace Camelotia.Presentation.Interfaces;

/// <summary>One folder on the breadcrumb trail.</summary>
public interface IFolderViewModel : INotifyPropertyChanged
{
    /// <summary>Gets the display name.</summary>
    string Name { get; }

    /// <summary>Gets the full path of the folder.</summary>
    string FullPath { get; }

    /// <summary>Gets the subfolders.</summary>
    IEnumerable<IFolderViewModel> Children { get; }

    /// <summary>Gets the provider view model the folder belongs to. Its commands move the provider to this folder.</summary>
    ICloudViewModel Provider { get; }
}
