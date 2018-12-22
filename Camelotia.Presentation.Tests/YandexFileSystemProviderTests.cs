using Camelotia.Services.Interfaces;
using Camelotia.Services.Providers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class YandexFileSystemProviderTests
    {
        private readonly IAuthenticator _authenticator = Substitute.For<IAuthenticator>();
        private readonly ITokenStorage _tokenStorage = Substitute.For<ITokenStorage>();

        [Fact]
        public void ShouldImplementNonNullInitialPath()
        {
            var provider = new YandexFileSystemProvider(_authenticator, _tokenStorage);
            provider.InitialPath.Should().NotBeNull();
        }
    }
}