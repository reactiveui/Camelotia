using System;
using Akavache;
using Camelotia.Services.Providers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class VkontakteFileSystemProviderTests
    {
        private static readonly Guid VkontakteIdentifier = Guid.NewGuid();
        private readonly IBlobCache _blobCache = Substitute.For<IBlobCache>();
        
        [Fact]
        public void ShouldImplementNonNullInitialPath()
        {
            var provider = new VkDocsProvider(VkontakteIdentifier, _blobCache);
            provider.InitialPath.Should().NotBeNull();
            provider.Id.Should().Be(VkontakteIdentifier);
        }
    }
}