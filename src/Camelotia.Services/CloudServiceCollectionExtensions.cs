// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using Akavache;
using Camelotia.Services.Apis;
using Camelotia.Services.Apis.Vk;
using Camelotia.Services.Apis.Yandex;
using Camelotia.Services.Configuration;
using Camelotia.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Camelotia.Services;

/// <summary>Registers the storage providers and the Refit clients they call.</summary>
public static class CloudServiceCollectionExtensions
{
    /// <summary>The Yandex Disk REST API host.</summary>
    private static readonly Uri YandexDiskAddress = new("https://cloud-api.yandex.net");

    /// <summary>The Yandex OAuth host.</summary>
    private static readonly Uri YandexOAuthAddress = new("https://oauth.yandex.com");

    /// <summary>The VK API host.</summary>
    private static readonly Uri VkAddress = new("https://api.vk.ru");

    /// <summary>The VK ID host.</summary>
    private static readonly Uri VkIdAddress = new("https://id.vk.ru");

    /// <summary>Extends a service collection with the Camelotia registrations.</summary>
    /// <param name="services">The service collection.</param>
    extension(IServiceCollection services)
    {
        /// <summary>Registers the generated Refit clients, the browser authenticator and the <see cref="ICloudFactory"/>.</summary>
        /// <returns>The same service collection.</returns>
        /// <remarks>The collection must already hold a <see cref="CloudConfiguration"/> and an <see cref="IBlobCache"/>.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IServiceCollection AddCloudServices() => services.AddCloudServices(static _ => { });

        /// <summary>Registers the generated Refit clients, the browser authenticator and the <see cref="ICloudFactory"/>.</summary>
        /// <param name="configureHttpClient">Configures each Refit client's HTTP pipeline, for example to add logging or retry handlers.</param>
        /// <returns>The same service collection.</returns>
        /// <remarks>The collection must already hold a <see cref="CloudConfiguration"/> and an <see cref="IBlobCache"/>.</remarks>
        public IServiceCollection AddCloudServices(Action<IHttpClientBuilder> configureHttpClient)
        {
            ArgumentNullException.ThrowIfNull(configureHttpClient);
            configureHttpClient(services.AddRefitGeneratedClient<IYandexDiskApi>(YandexJsonContext.Default).ConfigureHttpClient(static client => client.BaseAddress = YandexDiskAddress));
            configureHttpClient(services.AddRefitGeneratedClient<IYandexOAuthApi>(YandexJsonContext.Default).ConfigureHttpClient(static client => client.BaseAddress = YandexOAuthAddress));
            configureHttpClient(services.AddRefitGeneratedClient<IVkApi>(VkJsonContext.Default).ConfigureHttpClient(static client => client.BaseAddress = VkAddress));
            configureHttpClient(services.AddRefitGeneratedClient<IVkOAuthApi>(VkJsonContext.Default).ConfigureHttpClient(static client => client.BaseAddress = VkIdAddress));

            _ = services.AddSingleton(static provider => new CloudApis(
                provider.GetRequiredService<IYandexDiskApi>(),
                provider.GetRequiredService<IYandexOAuthApi>(),
                provider.GetRequiredService<IVkApi>(),
                provider.GetRequiredService<IVkOAuthApi>()));
            _ = services.AddSingleton<IAuthenticator, LoopbackBrowserAuthenticator>();
            _ = services.AddSingleton<ICloudFactory>(static provider => new CloudFactory(
                provider.GetRequiredService<CloudConfiguration>(),
                provider.GetRequiredService<IAuthenticator>(),
                provider.GetRequiredService<IBlobCache>(),
                provider.GetRequiredService<CloudApis>()));
            return services;
        }
    }
}
