using Camelotia.Services.Interfaces;
using Camelotia.Services.Providers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class VkontakteFileSystemProviderTests
    {
        private readonly ITokenStorage _tokenStorage = Substitute.For<ITokenStorage>();

        [Fact]
        public void ShouldImplementNonNullInitialPath()
        {
            var provider = new VkontakteFileSystemProvider(_tokenStorage);
            provider.InitialPath.Should().NotBeNull();
        }
    }
}