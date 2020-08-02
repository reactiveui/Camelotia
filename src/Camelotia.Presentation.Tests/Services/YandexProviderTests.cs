using System;
using System.IO;
using Akavache;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Camelotia.Presentation.Tests.Services
{
    public sealed class YandexFileSystemProviderTests
    {
        private readonly IAuthenticator _authenticator = Substitute.For<IAuthenticator>();
        private readonly IBlobCache _blobCache = Substitute.For<IBlobCache>();
        private readonly ProviderModel _model = new ProviderModel
        {
            Id = Guid.NewGuid(),
            Type = "Yandex",
            Created = DateTime.Now
        };

        [Fact]
        public void VerifyDefaultPropertyValues()
        {
            var provider = new YandexDiskProvider(_model, _authenticator, _blobCache);
            provider.InitialPath.Should().Be(Path.DirectorySeparatorChar.ToString());

            provider.CanCreateFolder.Should().BeTrue();
            provider.Created.Should().Be(_model.Created);
            provider.Name.Should().Be("Yandex");
            provider.Id.Should().Be(_model.Id);

            provider.SupportsDirectAuth.Should().BeFalse();
            provider.SupportsHostAuth.Should().BeFalse();
            provider.SupportsOAuth.Should().BeTrue();
        }
    }
}