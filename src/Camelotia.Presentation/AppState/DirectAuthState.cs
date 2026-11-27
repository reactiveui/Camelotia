// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.Serialization;

namespace Camelotia.Presentation.AppState;

/// <summary>The saved contents of the user name and password form.</summary>
[System.Diagnostics.DebuggerDisplay("DirectAuthState: {Username}")]
[DataContract]
public sealed class DirectAuthState
{
    /// <summary>Gets or sets the typed user name.</summary>
    [DataMember]
    public string Username { get; set; } = string.Empty;

    /// <summary>Gets or sets the typed password.</summary>
    [DataMember]
    public string Password { get; set; } = string.Empty;
}
