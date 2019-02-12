using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Providers;
using Camelotia.Services.Storages;
using DynamicData;
using DynamicData.Tests;
using NSubstitute;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class ProviderStorageTests
    {
        private readonly IAuthenticator _authenticator = Substitute.For<IAuthenticator>();
        private readonly ITokenStorage _tokenStorage = Substitute.For<ITokenStorage>();

        [Fact]
        public async Task ShouldResolveAllSupportedProviders()
        {
            var provider = new ProviderStorage(
                new LocalFileSystemProvider(),
                new VkontakteFileSystemProvider(_tokenStorage),
                new YandexFileSystemProvider(_authenticator, _tokenStorage)
            );

            await provider.LoadProviders();
            var providers = provider.Connect().AsAggregator();
            
            Assert.Equal(3, providers.Data.Count);
            Assert.Contains(providers.Data.Items, x => x is LocalFileSystemProvider);
            Assert.Contains(providers.Data.Items, x => x is VkontakteFileSystemProvider);
            Assert.Contains(providers.Data.Items, x => x is YandexFileSystemProvider);
        }

        [Fact]
        public async Task ShouldResolveOnlySpecifiedProvidersIfNeeded()
        {
            var provider = new ProviderStorage(new LocalFileSystemProvider());
            await provider.LoadProviders();
            var providers = provider.Connect().AsAggregator();

            Assert.Equal(1, providers.Data.Count);
            Assert.Contains(providers.Data.Items, x => x is LocalFileSystemProvider);
            Assert.DoesNotContain(providers.Data.Items, x => x is VkontakteFileSystemProvider);
            Assert.DoesNotContain(providers.Data.Items, x => x is YandexFileSystemProvider);
        }
    }
}