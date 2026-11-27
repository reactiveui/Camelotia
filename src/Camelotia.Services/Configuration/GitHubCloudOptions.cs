// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.Serialization;

namespace Camelotia.Services.Configuration;

/// <summary>The GitHub application settings.</summary>
[System.Diagnostics.DebuggerDisplay("GitHubCloudOptions: {GithubApplicationId}")]
[DataContract]
public class GitHubCloudOptions
{
    /// <summary>Gets or sets the product name sent in the user agent header.</summary>
    [DataMember]
    public string GithubApplicationId { get; set; } = "my-cool-app";
}
