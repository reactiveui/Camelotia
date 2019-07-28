using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using Camelotia.Services.Storages;
using DynamicData.Tests;
using NSubstitute;
using Akavache;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class ProviderStorageTests
    {
        private readonly IAuthenticator _authenticator = Substitute.For<IAuthenticator>();
        private readonly IObjectBlobCache _blobCache = Substitute.For<IObjectBlobCache>();

        [Fact]
        public async Task ShouldResolveAllSupportedProviders()
        {
            var provider = new ProviderStorage(new Dictionary<string, Func<ProviderModel, IProvider>>
            {
                [typeof(LocalProvider).Name] = id => new LocalProvider(id),
                [typeof(VkDocsProvider).Name] = id => new VkDocsProvider(id, _blobCache),
                [typeof(YandexDiskProvider).Name] = id => new YandexDiskProvider(id, _authenticator, _blobCache),
            }, _blobCache);

            await provider.Add(typeof(LocalProvider).Name);
            await provider.Add(typeof(VkDocsProvider).Name);
            await provider.Add(typeof(YandexDiskProvider).Name);
            var providers = provider.Read().AsAggregator();
            
            Assert.Equal(3, providers.Data.Count);
            Assert.Contains(providers.Data.Items, x => x is LocalProvider);
            Assert.Contains(providers.Data.Items, x => x is VkDocsProvider);
            Assert.Contains(providers.Data.Items, x => x is YandexDiskProvider);
        }

        [Fact]
        public async Task ShouldResolveOnlySpecifiedProvidersIfNeeded()
        {
            var provider = new ProviderStorage(new Dictionary<string, Func<ProviderModel, IProvider>>
            {
                [typeof(LocalProvider).Name] = id => new LocalProvider(id),
            }, _blobCache);
            
            await provider.Add(typeof(LocalProvider).Name);
            var providers = provider.Read().AsAggregator();

            Assert.Equal(1, providers.Data.Count);
            Assert.Contains(providers.Data.Items, x => x is LocalProvider);
            Assert.DoesNotContain(providers.Data.Items, x => x is VkDocsProvider);
            Assert.DoesNotContain(providers.Data.Items, x => x is YandexDiskProvider);
        }

        [Fact]
        public async Task ShouldRemoveProviders()
        {
            var provider = new ProviderStorage(new Dictionary<string, Func<ProviderModel, IProvider>>
            {
                [typeof(LocalProvider).Name] = id => new LocalProvider(id),
            }, _blobCache);
            
            await provider.Add(typeof(LocalProvider).Name);
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
                    Type = typeof(LocalProvider).Name
                }
            }));
            
            var provider = new ProviderStorage(new Dictionary<string, Func<ProviderModel, IProvider>>
            {
                [typeof(LocalProvider).Name] = id => new LocalProvider(id),
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