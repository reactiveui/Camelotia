using System;
using System.IO;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Camelotia.Tests.Services
{
    public sealed class YandexFileSystemProviderTests
    {
        private readonly IAuthenticator _authenticator = Substitute.For<IAuthenticator>();
        private readonly ProviderModel _model = new ProviderModel
        {
            Id = Guid.NewGuid(),
            Type = ProviderType.Yandex,
            Created = DateTime.Now
        };

        [Fact]
        public void VerifyDefaultPropertyValues()
        {
            var provider = new YandexDiskProvider(_model, _authenticator);
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