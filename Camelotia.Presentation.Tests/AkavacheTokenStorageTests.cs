using System.Threading.Tasks;
using Camelotia.Services.Providers;
using Camelotia.Services.Storages;
using FluentAssertions;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class AkavacheTokenStorageTests
    {
        [Fact]
        public async Task ShouldReadAndWriteTokens()
        {
            var storage = new AkavacheTokenStorage();
            await storage.WriteToken<VkontakteFileSystemProvider>("42");
            var result = await storage.ReadToken<VkontakteFileSystemProvider>();
            result.Should().NotBeNull();
            result.Should().Be("42");
        }

        [Fact]
        public async Task ShouldReadAndWriteNullsWithNoExceptions()
        {
            var storage = new AkavacheTokenStorage();
            await storage.WriteToken<VkontakteFileSystemProvider>(null);
            var result = await storage.ReadToken<VkontakteFileSystemProvider>();
            result.Should().BeNull();
        }

        [Fact]
        public async Task ShouldReadAndWriteMultipleTokens()
        {
            var storage = new AkavacheTokenStorage();
            await storage.WriteToken<VkontakteFileSystemProvider>("vk");
            await storage.WriteToken<YandexFileSystemProvider>("ya");
            var vk = await storage.ReadToken<VkontakteFileSystemProvider>();
            var ya = await storage.ReadToken<YandexFileSystemProvider>();
            vk.Should().Be("vk");
            ya.Should().Be("ya");
        }

        [Fact]
        public async Task ShouldReturnNullIfAttemptingToReadTokenThatDoesNotExistYet()
        {
            var storage = new AkavacheTokenStorage();
            var response = await storage.ReadToken<AkavacheTokenStorageTests>();
            response.Should().BeNull();
        }
    }
}