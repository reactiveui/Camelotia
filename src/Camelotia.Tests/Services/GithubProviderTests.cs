using System;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using FluentAssertions;
using Xunit;

namespace Camelotia.Tests.Services
{
    public sealed class GithubProviderTests
    {
        private readonly ProviderParameters _model = new ProviderParameters
        {
            Id = Guid.NewGuid(),
            Created = DateTime.Now,
            Type = ProviderType.GitHub
        };

        [Fact]
        public void VerifyDefaultPropertyValues()
        {
            var provider = new GitHubProvider(_model);
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