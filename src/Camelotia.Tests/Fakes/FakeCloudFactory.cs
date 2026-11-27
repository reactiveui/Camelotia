// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;

namespace Camelotia.Tests.Fakes;

/// <summary>An <see cref="ICloudFactory"/> that creates a <see cref="FakeCloud"/> for every saved provider.</summary>
/// <param name="supported">The provider types the factory reports.</param>
[System.Diagnostics.DebuggerDisplay("FakeCloudFactory: {Created.Count}")]
public sealed class FakeCloudFactory(IReadOnlyCollection<CloudType> supported) : ICloudFactory, IDisposable
{
    /// <summary>Initializes a new instance of the <see cref="FakeCloudFactory"/> class that supports the local and FTP types.</summary>
    public FakeCloudFactory()
        : this([CloudType.Local, CloudType.Ftp])
    {
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<CloudType> SupportedClouds { get; } = supported;

    /// <summary>Gets the providers the factory created, in order.</summary>
    public List<FakeCloud> Created { get; } = [];

    /// <inheritdoc/>
    public ICloud CreateCloud(CloudParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        var cloud = new FakeCloud { Id = parameters.Id, Created = parameters.Created, Parameters = parameters };
        Created.Add(cloud);
        return cloud;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var cloud in Created)
        {
            cloud.Dispose();
        }
    }
}
