// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.Serialization;
using Camelotia.Services.Models;

namespace Camelotia.Presentation.AppState;

/// <summary>The saved state of one provider and its browser.</summary>
[System.Diagnostics.DebuggerDisplay("CloudState: {Type} {Id}")]
[DataContract]
public sealed class CloudState
{
    /// <summary>Gets or sets the create-folder form state.</summary>
    [DataMember]
    public CreateFolderState CreateFolderState { get; set; } = new();

    /// <summary>Gets or sets the rename form state.</summary>
    [DataMember]
    public RenameFileState RenameFileState { get; set; } = new();

    /// <summary>Gets or sets the sign-in form state.</summary>
    [DataMember]
    public AuthState AuthState { get; set; } = new();

    /// <summary>Gets or sets the folder the browser was showing, or empty for the provider's start folder.</summary>
    [DataMember]
    public string CurrentPath { get; set; } = string.Empty;

    /// <summary>Gets or sets the provider instance identifier.</summary>
    [DataMember]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the time the user added the provider.</summary>
    [DataMember]
    public DateTimeOffset Created { get; set; }

    /// <summary>Gets or sets the provider type.</summary>
    [DataMember]
    public CloudType Type { get; set; } = CloudType.Local;

    /// <summary>Gets or sets the signed-in user, or <see langword="null"/> when signed out.</summary>
    [DataMember]
    public string? User { get; set; }

    /// <summary>Gets or sets the saved access token, or <see langword="null"/> when signed out.</summary>
    [DataMember]
    public string? Token { get; set; }

    /// <summary>Gets the parameters to create the provider from.</summary>
    [IgnoreDataMember]
    public CloudParameters Parameters => new() { Id = Id, Created = Created, Type = Type, User = User, Token = Token };
}
