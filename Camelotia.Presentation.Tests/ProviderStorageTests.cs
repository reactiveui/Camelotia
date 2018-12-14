using System.Linq;
using System.Threading.Tasks;
using Camelotia.Services.Providers;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class ProviderStorageTests
    {
        [Fact]
        public async Task ShouldResolveAllSupportedProviders()
        {
            var provider = new ProviderStorage();
            var sequence = await provider.LoadProviders();
            var providers = sequence.ToList();
            
            Assert.Contains(providers, x => x is LocalFileSystemProvider);
            Assert.Contains(providers, x => x is VkontakteFileSystemProvider);
            Assert.Contains(providers, x => x is YandexFileSystemProvider);
        }
    }
}