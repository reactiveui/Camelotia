﻿using System;
using System.IO;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using FluentAssertions;
using Xunit;

namespace Camelotia.Tests.Services
{
    public sealed class VkontakteFileSystemProviderTests
    {
        private readonly ProviderParameters _model = new ProviderParameters
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