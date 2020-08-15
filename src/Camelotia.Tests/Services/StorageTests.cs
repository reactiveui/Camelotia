using Akavache;
using Camelotia.Services;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Camelotia.Tests.Services
{
    public sealed class ProviderFactoryTests
    {
        private readonly IAuthenticator _authenticator = Substitute.For<IAuthenticator>();
        private readonly IObjectBlobCache _blobCache = Substitute.For<IObjectBlobCache>();
        
        [Fact]
        public void SupportedProviderTypesShouldNotBeEmpty()
        {
            var factory = new ProviderFactory(_authenticator, _blobCache);
            factory.SupportedTypes.Should().NotBeEmpty();
            factory.SupportedTypes.Should().Contain(ProviderType.Local);
            factory.SupportedTypes.Should().Contain(ProviderType.GitHub);
        }

        [Fact]
        public void ShouldInstantiateSupportedProviders()
        {
            var factory = new ProviderFactory(_authenticator, _blobCache);
            var provider = factory.CreateProvider(new ProviderModel {Type = ProviderType.Local});
            provider.Should().NotBeNull();
            provider.Name.Should().Be(ProviderType.Local.ToString());
        }
    }
}