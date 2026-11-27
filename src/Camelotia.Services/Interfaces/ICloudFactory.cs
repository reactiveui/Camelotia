// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Services.Models;

namespace Camelotia.Services.Interfaces;

/// <summary>Creates storage providers from saved parameters.</summary>
public interface ICloudFactory
{
    /// <summary>Gets the provider types the user can add.</summary>
    IReadOnlyCollection<CloudType> SupportedClouds { get; }

    /// <summary>Creates the provider the parameters describe.</summary>
    /// <param name="parameters">The saved parameters.</param>
    /// <returns>The provider.</returns>
    ICloud CreateCloud(CloudParameters parameters);
}
