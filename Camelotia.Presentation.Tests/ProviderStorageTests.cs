using System.Linq;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Providers;
using Camelotia.Services.Storages;
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

            var sequence = await provider.LoadProviders();
            var providers = sequence.ToList();
            
            Assert.Contains(providers, x => x is LocalFileSystemProvider);
            Assert.Contains(providers, x => x is VkontakteFileSystemProvider);
            Assert.Contains(providers, x => x is YandexFileSystemProvider);
        }

        [Fact]
        public async Task ShouldResolveOnlySpecifiedProvidersIfNeeded()
        {
            var provider = new ProviderStorage(new LocalFileSystemProvider());
            var sequence = await provider.LoadProviders();
            var providers = sequence.ToList();

            Assert.Contains(providers, x => x is LocalFileSystemProvider);
            Assert.DoesNotContain(providers, x => x is VkontakteFileSystemProvider);
            Assert.DoesNotContain(providers, x => x is YandexFileSystemProvider);
        }
    }
}