// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.Serialization;

namespace Camelotia.Services.Configuration;

/// <summary>The Yandex Disk application settings.</summary>
[System.Diagnostics.DebuggerDisplay("YandexDiskCloudOptions: {CodeAuthClientId}")]
[DataContract]
public class YandexDiskCloudOptions
{
    /// <summary>Gets or sets the client secret for the authorization-code grant.</summary>
    [DataMember]
    public string CodeAuthClientSecret { get; set; } = "317a14f5491447e8bd3a9e7e14ce46cd";

    /// <summary>Gets or sets the client identifier for the authorization-code grant.</summary>
    [DataMember]
    public string CodeAuthClientId { get; set; } = "122661520b174cb5b85b4a3c26aa66f6";
}
