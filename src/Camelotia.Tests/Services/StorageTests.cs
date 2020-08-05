using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using Camelotia.Services.Storages;
using DynamicData.Tests;
using NSubstitute;
using Xunit;

namespace Camelotia.Tests.Services
{
    public sealed class StorageTests
    {
        private readonly IAuthenticator _authenticator = Substitute.For<IAuthenticator>();
        private readonly IObjectBlobCache _blobCache = Substitute.For<IObjectBlobCache>();

        [Fact]
        public async Task ShouldResolveAllSupportedProviders()
        {
            var provider = new AkavacheStorage(new Dictionary<string, Func<ProviderModel, IProvider>>
            {
                [nameof(LocalProvider)] = id => new LocalProvider(id),
                [nameof(VkDocsProvider)] = id => new VkDocsProvider(id, _blobCache),
                [nameof(YandexDiskProvider)] = id => new YandexDiskProvider(id, _authenticator, _blobCache),
            }, _blobCache);

            await provider.Add(nameof(LocalProvider));
            await provider.Add(nameof(VkDocsProvider));
            await provider.Add(nameof(YandexDiskProvider));
            var providers = provider.Read().AsAggregator();
            
            Assert.Equal(3, providers.Data.Count);
            Assert.Contains(providers.Data.Items, x => x is LocalProvider);
            Assert.Contains(providers.Data.Items, x => x is VkDocsProvider);
            Assert.Contains(providers.Data.Items, x => x is YandexDiskProvider);
        }

        [Fact]
        public async Task ShouldResolveOnlySpecifiedProvidersIfNeeded()
        {
            var provider = new AkavacheStorage(new Dictionary<string, Func<ProviderModel, IProvider>>
            {
                [nameof(LocalProvider)] = id => new LocalProvider(id),
            }, _blobCache);
            
            await provider.Add(nameof(LocalProvider));
            var providers = provider.Read().AsAggregator();

            Assert.Equal(1, providers.Data.Count);
            Assert.Contains(providers.Data.Items, x => x is LocalProvider);
            Assert.DoesNotContain(providers.Data.Items, x => x is VkDocsProvider);
            Assert.DoesNotContain(providers.Data.Items, x => x is YandexDiskProvider);
        }

        [Fact]
        public async Task ShouldRemoveProviders()
        {
            var provider = new AkavacheStorage(new Dictionary<string, Func<ProviderModel, IProvider>>
            {
                [nameof(LocalProvider)] = id => new LocalProvider(id),
            }, _blobCache);
            
            await provider.Add(nameof(LocalProvider));
            var providers = provider.Read().AsAggregator();
            Assert.Equal(1, providers.Data.Count);

            await provider.Remove(providers.Data.Items.First().Id);
            Assert.Equal(0, providers.Data.Count);
        }

        [Fact]
        public async Task ShouldReloadAllStuffFromBlobCache()
        {
            var identity = Guid.NewGuid();
            _blobCache.GetAllObjects<ProviderModel>().Returns(Observable.Return(new[]
            {
                new ProviderModel
                {
                    Id = identity,
                    Token = "12345",
                    Type = nameof(LocalProvider)
                }
            }));
            
            var provider = new AkavacheStorage(new Dictionary<string, Func<ProviderModel, IProvider>>
            {
                [nameof(LocalProvider)] = id => new LocalProvider(id),
            }, _blobCache);

            await provider.Refresh();
            var providers = provider.Read().AsAggregator();
            Assert.Equal(1, providers.Data.Count);
            Assert.Contains(providers.Data.Items, x => x is LocalProvider);
            Assert.DoesNotContain(providers.Data.Items, x => x is VkDocsProvider);
            Assert.DoesNotContain(providers.Data.Items, x => x is YandexDiskProvider);
            Assert.Equal(identity, providers.Data.Items.First().Id);
        }
    }
}