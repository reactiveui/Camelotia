using System;
using Akavache;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Providers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class YandexFileSystemProviderTests
    {
        private static readonly Guid YandexIdentifier = Guid.NewGuid();
        private readonly IAuthenticator _authenticator = Substitute.For<IAuthenticator>();
        private readonly IBlobCache _blobCache = Substitute.For<IBlobCache>();

        [Fact]
        public void ShouldImplementNonNullInitialPath()
        {
            var provider = new YandexDiskProvider(YandexIdentifier, _authenticator, _blobCache);
            provider.InitialPath.Should().NotBeNull();
            provider.Id.Should().Be(YandexIdentifier);
        }
    }
}