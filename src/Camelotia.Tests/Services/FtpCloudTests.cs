using System;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using FluentAssertions;
using Xunit;

namespace Camelotia.Tests.Services;

public sealed class FtpCloudTests
{
    private readonly CloudParameters _model = new()
    {
        Id = Guid.NewGuid(),
        Created = DateTime.Now,
        Type = CloudType.Ftp
    };

    [Fact]
    public void VerifyDefaultPropertyValues()
    {
        var provider = new FtpCloud(_model);
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
