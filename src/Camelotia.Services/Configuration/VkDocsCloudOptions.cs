// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.Serialization;

namespace Camelotia.Services.Configuration;

/// <summary>The VK application settings.</summary>
[System.Diagnostics.DebuggerDisplay("VkDocsCloudOptions: {ApplicationId}")]
[DataContract]
public class VkDocsCloudOptions
{
    /// <summary>Gets or sets the VK application identifier.</summary>
    [DataMember]
    public ulong ApplicationId { get; set; } = 5_560_698;
}
