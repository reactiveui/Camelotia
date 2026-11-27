// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Akavache;
using Akavache.SystemTextJson;
using Camelotia.Services;
using Camelotia.Services.Apis;
using Camelotia.Services.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Camelotia.Tests.Infrastructure;

/// <summary>Builds the app's real service registrations, with every HTTP request answered by a stub handler.</summary>
[System.Diagnostics.DebuggerDisplay("CloudTestServices")]
public sealed class CloudTestServices : IDisposable
{
    /// <summary>The built container.</summary>
    private readonly ServiceProvider _provider;

    /// <summary>Initializes a new instance of the <see cref="CloudTestServices"/> class.</summary>
    /// <param name="handler">Answers every HTTP request.</param>
    public CloudTestServices(StubHttpHandler handler)
    {
        Cache = new InMemoryBlobCache(new SystemJsonSerializer());
        _provider = new ServiceCollection()
            .AddSingleton(Configuration)
            .AddSingleton(Cache)
            .AddCloudServices(builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler))
            .BuildServiceProvider();
    }

    /// <summary>Gets the application keys the providers use.</summary>
    public CloudConfiguration Configuration { get; } = new();

    /// <summary>Gets the in-memory cache the providers use.</summary>
    public IBlobCache Cache { get; }

    /// <summary>Gets the generated Refit clients.</summary>
    public CloudApis Apis => _provider.GetRequiredService<CloudApis>();

    /// <summary>Gets the registered cloud factory.</summary>
    public Camelotia.Services.Interfaces.ICloudFactory Factory => _provider.GetRequiredService<Camelotia.Services.Interfaces.ICloudFactory>();

    /// <summary>Gets the registered authenticator.</summary>
    public Camelotia.Services.Interfaces.IAuthenticator Authenticator => _provider.GetRequiredService<Camelotia.Services.Interfaces.IAuthenticator>();

    /// <inheritdoc/>
    public void Dispose()
    {
        _provider.Dispose();
        Cache.Dispose();
    }
}
