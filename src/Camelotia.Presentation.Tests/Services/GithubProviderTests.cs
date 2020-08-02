using System;
using Akavache;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Camelotia.Presentation.Tests.Services
{
    public sealed class GithubProviderTests
    {
        private readonly IBlobCache _blobCache = Substitute.For<IBlobCache>();
        private readonly ProviderModel _model = new ProviderModel
        {
            Id = Guid.NewGuid(),
            Created = DateTime.Now,
            Type = "GitHub"
        };

        [Fact]
        public void VerifyDefaultPropertyValues()
        {
            var provider = new GitHubProvider(_model, _blobCache);
            provider.InitialPath.Should().Be(string.Empty);

            provider.CanCreateFolder.Should().BeFalse();
            provider.Created.Should().Be(_model.Created);
            provider.Name.Should().Be("GitHub");
            provider.Id.Should().Be(_model.Id);

            provider.SupportsDirectAuth.Should().BeTrue();
            provider.SupportsHostAuth.Should().BeFalse();
            provider.SupportsOAuth.Should().BeFalse();
        }
    }
}