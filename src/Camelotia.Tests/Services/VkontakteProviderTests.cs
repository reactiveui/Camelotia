using System;
using System.IO;
using Akavache;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Camelotia.Tests.Services
{
    public sealed class VkontakteFileSystemProviderTests
    {
        private readonly ProviderModel _model = new ProviderModel
        {
            Id = Guid.NewGuid(),
            Type = ProviderType.VkDocs,
            Created = DateTime.Now
        };

        [Fact]
        public void VerifyDefaultPropertyValues()
        {
            var provider = new VkDocsProvider(_model);
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
}