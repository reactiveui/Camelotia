// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Akavache;
using Camelotia.Services.Apis;
using Camelotia.Services.Configuration;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;

namespace Camelotia.Services;

/// <summary>Creates the provider that saved parameters describe.</summary>
[System.Diagnostics.DebuggerDisplay("CloudFactory: {SupportedClouds}")]
public sealed class CloudFactory : ICloudFactory
{
    /// <summary>Every provider type, in the order the add menu lists them.</summary>
    private static readonly CloudType[] AllClouds =
    [
        CloudType.Local,
        CloudType.Ftp,
        CloudType.Sftp,
        CloudType.Yandex,
        CloudType.GitHub,
        CloudType.GoogleDrive,
        CloudType.VkDocs,
    ];

    /// <summary>The application keys for each provider.</summary>
    private readonly CloudConfiguration _configuration;

    /// <summary>Runs browser sign-ins for the OAuth providers.</summary>
    private readonly IAuthenticator _authenticator;

    /// <summary>The cache that stores provider credentials.</summary>
    private readonly IBlobCache _cache;

    /// <summary>The generated Refit clients the HTTP providers call.</summary>
    private readonly CloudApis _apis;

    /// <summary>Initializes a new instance of the <see cref="CloudFactory"/> class that supports every provider type.</summary>
    /// <param name="configuration">The application keys for each provider.</param>
    /// <param name="authenticator">Runs browser sign-ins.</param>
    /// <param name="cache">The cache that stores provider credentials.</param>
    /// <param name="apis">The generated Refit clients the HTTP providers call.</param>
    public CloudFactory(CloudConfiguration configuration, IAuthenticator authenticator, IBlobCache cache, CloudApis apis)
        : this(configuration, authenticator, cache, apis, AllClouds)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="CloudFactory"/> class.</summary>
    /// <param name="configuration">The application keys for each provider.</param>
    /// <param name="authenticator">Runs browser sign-ins.</param>
    /// <param name="cache">The cache that stores provider credentials.</param>
    /// <param name="apis">The generated Refit clients the HTTP providers call.</param>
    /// <param name="supported">The provider types the user can add.</param>
    public CloudFactory(CloudConfiguration configuration, IAuthenticator authenticator, IBlobCache cache, CloudApis apis, IReadOnlyCollection<CloudType> supported)
    {
        _configuration = configuration;
        _authenticator = authenticator;
        _cache = cache;
        _apis = apis;
        SupportedClouds = supported;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<CloudType> SupportedClouds { get; }

    /// <inheritdoc/>
    public ICloud CreateCloud(CloudParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        return parameters.Type switch
        {
            CloudType.Ftp => new FtpCloud(parameters),
            CloudType.GitHub => new GitHubCloud(parameters, _configuration.GitHub),
            CloudType.GoogleDrive => new GoogleDriveCloud(parameters, _cache, _configuration.GoogleDrive),
            CloudType.Local => new LocalCloud(parameters),
            CloudType.Sftp => new SftpCloud(parameters),
            CloudType.VkDocs => new VkDocsCloud(parameters, _authenticator, _configuration.VkDocs, _apis.Vk, _apis.VkId),
            CloudType.Yandex => new YandexDiskCloud(parameters, _authenticator, _configuration.YandexDisk, _apis.YandexDisk, _apis.YandexOAuth),
            _ => throw new ArgumentOutOfRangeException(nameof(parameters), parameters.Type, "Unknown provider type."),
        };
    }
}
