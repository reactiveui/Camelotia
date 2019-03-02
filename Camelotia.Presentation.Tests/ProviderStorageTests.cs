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
            var provider = new ProviderStorage(new Dictionary<string, Func<Guid, IProvider>>
            {
                [typeof(LocalFileSystemProvider).Name] = id => new LocalFileSystemProvider(id),
                [typeof(VkontakteFileSystemProvider).Name] = id => new VkontakteFileSystemProvider(id, _blobCache),
                [typeof(YandexFileSystemProvider).Name] = id => new YandexFileSystemProvider(id, _authenticator, _blobCache),
            }, _blobCache);

            await provider.Add(typeof(LocalFileSystemProvider).Name);
            await provider.Add(typeof(VkontakteFileSystemProvider).Name);
            await provider.Add(typeof(YandexFileSystemProvider).Name);
            var providers = provider.Providers().AsAggregator();
            
            Assert.Equal(3, providers.Data.Count);
            Assert.Contains(providers.Data.Items, x => x is LocalFileSystemProvider);
            Assert.Contains(providers.Data.Items, x => x is VkontakteFileSystemProvider);
            Assert.Contains(providers.Data.Items, x => x is YandexFileSystemProvider);
        }

        [Fact]
        public async Task ShouldResolveOnlySpecifiedProvidersIfNeeded()
        {
            var provider = new ProviderStorage(new Dictionary<string, Func<Guid, IProvider>>
            {
                [typeof(LocalFileSystemProvider).Name] = id => new LocalFileSystemProvider(id),
            }, _blobCache);
            
            await provider.Add(typeof(LocalFileSystemProvider).Name);
            var providers = provider.Providers().AsAggregator();

            Assert.Equal(1, providers.Data.Count);
            Assert.Contains(providers.Data.Items, x => x is LocalFileSystemProvider);
            Assert.DoesNotContain(providers.Data.Items, x => x is VkontakteFileSystemProvider);
            Assert.DoesNotContain(providers.Data.Items, x => x is YandexFileSystemProvider);
        }

        [Fact]
        public async Task ShouldRemoveProviders()
        {
            var provider = new ProviderStorage(new Dictionary<string, Func<Guid, IProvider>>
            {
                [typeof(LocalFileSystemProvider).Name] = id => new LocalFileSystemProvider(id),
            }, _blobCache);
            
            await provider.Add(typeof(LocalFileSystemProvider).Name);
            var providers = provider.Providers().AsAggregator();
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
                    Type = typeof(LocalFileSystemProvider).Name
                }
            }));
            
            var provider = new ProviderStorage(new Dictionary<string, Func<Guid, IProvider>>
            {
                [typeof(LocalFileSystemProvider).Name] = id => new LocalFileSystemProvider(id),
            }, _blobCache);

            await provider.Refresh();
            var providers = provider.Providers().AsAggregator();
            Assert.Equal(1, providers.Data.Count);
            Assert.Contains(providers.Data.Items, x => x is LocalFileSystemProvider);
            Assert.DoesNotContain(providers.Data.Items, x => x is VkontakteFileSystemProvider);
            Assert.DoesNotContain(providers.Data.Items, x => x is YandexFileSystemProvider);
            Assert.Equal(identity, providers.Data.Items.First().Id);
        }
    }
}