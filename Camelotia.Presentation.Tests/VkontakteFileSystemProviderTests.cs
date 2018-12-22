using Camelotia.Services.Interfaces;
using Camelotia.Services.Providers;
using FluentAssertions;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class VkontakteFileSystemProviderTests
    {
        [Fact]
        public void ShouldImplementNonNullInitialPath()
        {
            var provider = new VkontakteFileSystemProvider();
            provider.InitialPath.Should().NotBeNull();
        }
    }
}