// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
namespace Camelotia.Services.Models;

/// <summary>A file or folder that a provider lists.</summary>
[System.Diagnostics.DebuggerDisplay("FileModel: {Name}")]
public class FileModel
{
    /// <summary>Gets or sets the display name.</summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets the provider-specific path or identifier.</summary>
    public required string Path { get; set; }

    /// <summary>Gets or sets the size in bytes.</summary>
    public long Size { get; set; }

    /// <summary>Gets or sets a value indicating whether the entry is a folder.</summary>
    public bool IsFolder { get; set; }

    /// <summary>Gets or sets the last change time, or <see langword="null"/> when the provider does not report it.</summary>
    public DateTimeOffset? Modified { get; set; }
}
