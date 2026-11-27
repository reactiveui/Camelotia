// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
namespace Camelotia.Services.Models;

/// <summary>The saved settings of one storage provider.</summary>
[System.Diagnostics.DebuggerDisplay("CloudParameters: {Id}")]
public class CloudParameters
{
    /// <summary>Gets or sets the unique identifier of the provider instance.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the time the user added the provider.</summary>
    public DateTimeOffset Created { get; set; }

    /// <summary>Gets or sets the provider type.</summary>
    public CloudType Type { get; set; } = CloudType.Local;

    /// <summary>Gets or sets the signed-in user, or <see langword="null"/> when signed out.</summary>
    public string? User { get; set; }

    /// <summary>Gets or sets the saved access token, or <see langword="null"/> when signed out.</summary>
    public string? Token { get; set; }
}
