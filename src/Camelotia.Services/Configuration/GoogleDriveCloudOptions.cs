// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.Serialization;

namespace Camelotia.Services.Configuration;

/// <summary>The Google Drive application settings.</summary>
[System.Diagnostics.DebuggerDisplay("GoogleDriveCloudOptions: {GoogleDriveApplicationName}")]
[DataContract]
public class GoogleDriveCloudOptions
{
    /// <summary>Gets or sets the application name sent to Google.</summary>
    [DataMember]
    public string GoogleDriveApplicationName { get; set; } = "Camelotia";

    /// <summary>Gets or sets the OAuth client identifier.</summary>
    [DataMember]
    public string GoogleDriveClientId { get; set; } = "1096201018044-qbv35mo5cd7b5utfjpg83v5lsuhssvvg.apps.googleusercontent.com";

    /// <summary>Gets or sets the OAuth client secret.</summary>
    [DataMember]
    public string GoogleDriveClientSecret { get; set; } = "L-xoeULle07kb_jHleqMxWo2";

    /// <summary>Gets or sets the local user name the credential is stored under.</summary>
    [DataMember]
    public string GoogleDriveUserName { get; set; } = "user";
}
