using System;
using Camelotia.Presentation.AppState;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using FluentAssertions;
using Xunit;

namespace Camelotia.Tests.Services;

public sealed class GithubCloudTests
{
    private readonly MainState _state = new();
    private readonly CloudParameters _model = new()
    {
        Id = Guid.NewGuid(),
        Created = DateTime.Now,
        Type = CloudType.GitHub
    };

    [Fact]
    public void VerifyDefaultPropertyValues()
    {
        var provider = new GitHubCloud(_model, _state.CloudConfiguration.GitHub);
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
