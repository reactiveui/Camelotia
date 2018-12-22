using Camelotia.Services.Interfaces;
using Camelotia.Services.Providers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class YandexFileSystemProviderTests
    {
        private readonly IYandexAuthenticator _authenticator = Substitute.For<IYandexAuthenticator>();

        [Fact]
        public void ShouldImplementNonNullInitialPath()
        {
            var provider = new YandexFileSystemProvider(_authenticator);
            provider.InitialPath.Should().NotBeNull();
        }
    }
}