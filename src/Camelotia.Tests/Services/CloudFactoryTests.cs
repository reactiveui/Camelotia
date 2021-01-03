using Akavache;
using Camelotia.Services;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Camelotia.Tests.Services
{
    public sealed class CloudFactoryTests
    {
        private readonly IAuthenticator _authenticator = Substitute.For<IAuthenticator>();
        private readonly IObjectBlobCache _blobCache = Substitute.For<IObjectBlobCache>();

        [Fact]
        public void SupportedProviderTypesShouldNotBeEmpty()
        {
            var factory = new CloudFactory(_authenticator, _blobCache);
            factory.SupportedClouds.Should().NotBeEmpty();
            factory.SupportedClouds.Should().Contain(CloudType.Local);
            factory.SupportedClouds.Should().Contain(CloudType.GitHub);
        }

        [Fact]
        public void ShouldInstantiateSupportedProviders()
        {
            var factory = new CloudFactory(_authenticator, _blobCache);
            var provider = factory.CreateCloud(new CloudParameters { Type = CloudType.Local });
            provider.Should().NotBeNull();
            provider.Name.Should().Be(CloudType.Local.ToString());
        }
    }
}