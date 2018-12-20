using Camelotia.Services.Interfaces;
using Camelotia.Services.Providers;
using FluentAssertions;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class VkontakteFileSystemProviderTests
    {
        private readonly IProvider _provider = new VkontakteFileSystemProvider();

        [Fact]
        public void ShouldImplementNonNullInitialPath() => _provider.InitialPath.Should().NotBeNull();
    }
}