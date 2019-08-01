using System;
using Akavache;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class GoogleDriveProviderTests
    {
        private readonly IBlobCache _blobCache = Substitute.For<IBlobCache>();
        private readonly ProviderModel _model = new ProviderModel
        {
            Id = Guid.NewGuid(),
            Created = DateTime.Now,
            Type = "Google Drive"
        };

        [Fact]
        public void VerifyDefaultPropertyValues()
        {
            var provider = new GoogleDriveProvider(_model, _blobCache);
            provider.InitialPath.Should().Be("/");

            provider.CanCreateFolder.Should().BeFalse();
            provider.Created.Should().Be(_model.Created);
            provider.Name.Should().Be("Google Drive");
            provider.Id.Should().Be(_model.Id);

            provider.SupportsDirectAuth.Should().BeFalse();
            provider.SupportsHostAuth.Should().BeFalse();
            provider.SupportsOAuth.Should().BeTrue();
        }
    }
}