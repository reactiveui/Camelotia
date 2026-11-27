// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.Serialization;

namespace Camelotia.Services.Configuration;

/// <summary>The application keys for each provider that needs one.</summary>
[System.Diagnostics.DebuggerDisplay("CloudConfiguration: {GitHub}")]
[DataContract]
public class CloudConfiguration
{
    /// <summary>Gets or sets the GitHub settings.</summary>
    [DataMember]
    public GitHubCloudOptions GitHub { get; set; } = new();

    /// <summary>Gets or sets the Google Drive settings.</summary>
    [DataMember]
    public GoogleDriveCloudOptions GoogleDrive { get; set; } = new();

    /// <summary>Gets or sets the VK settings.</summary>
    [DataMember]
    public VkDocsCloudOptions VkDocs { get; set; } = new();

    /// <summary>Gets or sets the Yandex Disk settings.</summary>
    [DataMember]
    public YandexDiskCloudOptions YandexDisk { get; set; } = new();
}
