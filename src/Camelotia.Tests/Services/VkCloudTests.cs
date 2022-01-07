using System;
using System.IO;
using Camelotia.Presentation.AppState;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using FluentAssertions;
using Xunit;

namespace Camelotia.Tests.Services;

public sealed class VkCloudTests
{
    private readonly MainState _state = new();
    private readonly CloudParameters _model = new()
    {
        Id = Guid.NewGuid(),
        Type = CloudType.VkDocs,
        Created = DateTime.Now
    };

    [Fact]
    public void VerifyDefaultPropertyValues()
    {
        var provider = new VkDocsCloud(_model, _state.CloudConfiguration.VkDocs);
        provider.InitialPath.Should().Be(Path.DirectorySeparatorChar.ToString());

        provider.CanCreateFolder.Should().BeFalse();
        provider.Created.Should().Be(_model.Created);
        provider.Name.Should().Be("VkDocs");
        provider.Id.Should().Be(_model.Id);

        provider.SupportsDirectAuth.Should().BeTrue();
        provider.SupportsHostAuth.Should().BeFalse();
        provider.SupportsOAuth.Should().BeFalse();
    }
}
