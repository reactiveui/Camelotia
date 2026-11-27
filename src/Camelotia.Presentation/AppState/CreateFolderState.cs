// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.Serialization;

namespace Camelotia.Presentation.AppState;

/// <summary>The saved contents of the create-folder form.</summary>
[System.Diagnostics.DebuggerDisplay("CreateFolderState: {Name}")]
[DataContract]
public sealed class CreateFolderState
{
    /// <summary>Gets or sets the typed folder name.</summary>
    [DataMember]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the form was open.</summary>
    [DataMember]
    public bool IsVisible { get; set; }
}
