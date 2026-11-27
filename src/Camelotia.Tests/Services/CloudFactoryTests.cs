// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Services;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using Camelotia.Tests.Infrastructure;

namespace Camelotia.Tests.Services;

/// <summary>Tests <see cref="CloudFactory"/> and the service registrations that build it.</summary>
public sealed class CloudFactoryTests
{
    /// <summary>Creates the provider type each saved parameter set names.</summary>
    /// <param name="type">The provider type.</param>
    /// <param name="expected">The expected implementation.</param>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    [Arguments(CloudType.Ftp, typeof(FtpCloud))]
    [Arguments(CloudType.GitHub, typeof(GitHubCloud))]
    [Arguments(CloudType.GoogleDrive, typeof(GoogleDriveCloud))]
    [Arguments(CloudType.Local, typeof(LocalCloud))]
    [Arguments(CloudType.Sftp, typeof(SftpCloud))]
    [Arguments(CloudType.VkDocs, typeof(VkDocsCloud))]
    [Arguments(CloudType.Yandex, typeof(YandexDiskCloud))]
    public async Task CreateCloud_CreatesTheNamedProvider(CloudType type, Type expected)
    {
        using var services = new CloudTestServices(new());

        var cloud = services.Factory.CreateCloud(new CloudParameters { Type = type });

        await Assert.That(cloud.GetType()).IsEqualTo(expected);
        (cloud as IDisposable)?.Dispose();
    }

    /// <summary>Lists every provider type by default, and only the chosen ones when told.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task SupportedClouds_DefaultsToEveryType()
    {
        using var services = new CloudTestServices(new());
        var limited = new CloudFactory(services.Configuration, services.Authenticator, services.Cache, services.Apis, [CloudType.Local]);

        await Assert.That(services.Factory.SupportedClouds.Count).IsEqualTo(Enum.GetValues<CloudType>().Length);
        await Assert.That(limited.SupportedClouds).IsEquivalentTo([CloudType.Local]);
    }

    /// <summary>Rejects missing parameters and unknown provider types.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task CreateCloud_RejectsBadParameters()
    {
        using var services = new CloudTestServices(new());

        _ = await Assert.That(() => services.Factory.CreateCloud(null!)).Throws<ArgumentNullException>();
        _ = await Assert.That(() => services.Factory.CreateCloud(new CloudParameters { Type = (CloudType)int.MaxValue })).Throws<ArgumentOutOfRangeException>();
    }

    /// <summary>Registers the browser authenticator and rejects a missing HTTP configuration callback.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task AddCloudServices_RegistersTheAuthenticator()
    {
        using var services = new CloudTestServices(new());
        var collection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        await Assert.That(services.Authenticator).IsTypeOf<LoopbackBrowserAuthenticator>();
        await Assert.That(collection.AddCloudServices()).IsSameReferenceAs(collection);
        _ = await Assert.That(() => collection.AddCloudServices(null!)).Throws<ArgumentNullException>();
    }

    /// <summary>Checks the interface the factory implements is the one the app consumes.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Factory_IsRegisteredAsTheInterface()
    {
        using var services = new CloudTestServices(new());

        await Assert.That(services.Factory).IsAssignableTo<ICloudFactory>();
    }
}
