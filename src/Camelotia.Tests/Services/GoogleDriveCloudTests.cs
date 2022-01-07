using System;
using Akavache;
using Camelotia.Presentation.AppState;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Camelotia.Tests.Services;

public sealed class GoogleDriveCloudTests
{
    private readonly MainState _state = new();
    private readonly IBlobCache _blobCache = Substitute.For<IBlobCache>();
    private readonly CloudParameters _model = new()
    {
        Id = Guid.NewGuid(),
        Created = DateTime.Now,
        Type = CloudType.GoogleDrive
    };

    [Fact]
    public void VerifyDefaultPropertyValues()
    {
        var provider = new GoogleDriveCloud(_model, _blobCache, _state.CloudConfiguration.GoogleDrive);
        provider.InitialPath.Should().Be("/");

        provider.CanCreateFolder.Should().BeFalse();
        provider.Created.Should().Be(_model.Created);
        provider.Name.Should().Be("GoogleDrive");
        provider.Id.Should().Be(_model.Id);

        provider.SupportsDirectAuth.Should().BeFalse();
        provider.SupportsHostAuth.Should().BeFalse();
        provider.SupportsOAuth.Should().BeTrue();
    }
}
