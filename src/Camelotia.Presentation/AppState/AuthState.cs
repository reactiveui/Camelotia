// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.Serialization;

namespace Camelotia.Presentation.AppState;

/// <summary>The saved contents of one provider's sign-in forms.</summary>
[System.Diagnostics.DebuggerDisplay("AuthState: {DirectAuthState}")]
[DataContract]
public sealed class AuthState
{
    /// <summary>Gets or sets the user name and password form state.</summary>
    [DataMember]
    public DirectAuthState DirectAuthState { get; set; } = new();

    /// <summary>Gets or sets the host sign-in form state.</summary>
    [DataMember]
    public HostAuthState HostAuthState { get; set; } = new();
}
