using System.Linq;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Providers;
using NSubstitute;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class ProviderStorageTests
    {
        private readonly IYandexAuthenticator _authenticator = Substitute.For<IYandexAuthenticator>();

        [Fact]
        public async Task ShouldResolveAllSupportedProviders()
        {
            var provider = new ProviderStorage(
                new LocalFileSystemProvider(),
                new VkontakteFileSystemProvider(),
                new YandexFileSystemProvider(_authenticator)
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