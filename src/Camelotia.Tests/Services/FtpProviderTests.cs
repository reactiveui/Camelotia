using System;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using FluentAssertions;
using Xunit;

namespace Camelotia.Tests.Services
{
    public sealed class FtpProviderTests
    {
        private readonly ProviderModel _model = new ProviderModel
        {
            Id = Guid.NewGuid(),
            Created = DateTime.Now,
            Type = ProviderType.Ftp
        };

        [Fact]
        public void VerifyDefaultPropertyValues()
        {
            var provider = new FtpProvider(_model);
            provider.InitialPath.Should().Be("/");

            provider.CanCreateFolder.Should().BeTrue();
            provider.Created.Should().Be(_model.Created);
            provider.Name.Should().Be("Ftp");
            provider.Id.Should().Be(_model.Id);

            provider.SupportsDirectAuth.Should().BeFalse();
            provider.SupportsHostAuth.Should().BeTrue();
            provider.SupportsOAuth.Should().BeFalse();
        }
    }
}