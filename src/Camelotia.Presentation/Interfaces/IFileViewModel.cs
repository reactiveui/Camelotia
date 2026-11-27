// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.ComponentModel;

namespace Camelotia.Presentation.Interfaces;

/// <summary>One file or folder in a provider's listing.</summary>
public interface IFileViewModel : INotifyPropertyChanged
{
    /// <summary>Gets the display name.</summary>
    string Name { get; }

    /// <summary>Gets the provider view model that listed the entry.</summary>
    ICloudViewModel Provider { get; }

    /// <summary>Gets the last change time, formatted for display.</summary>
    string Modified { get; }

    /// <summary>Gets a value indicating whether the entry is a folder.</summary>
    bool IsFolder { get; }

    /// <summary>Gets a value indicating whether the entry is a file.</summary>
    bool IsFile { get; }

    /// <summary>Gets the provider-specific path or identifier.</summary>
    string Path { get; }

    /// <summary>Gets the size, formatted for display.</summary>
    string Size { get; }
}
