using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using FluentAssertions;
using Xunit;

namespace Camelotia.Tests.Services
{
    public sealed class LocalFileSystemProviderTests
    {
        private static readonly Guid LocalIdentifier = Guid.NewGuid();
        private static readonly string Separator = Path.DirectorySeparatorChar.ToString();
        private readonly IProvider _provider = new LocalProvider(new ProviderModel
        {
            Id = LocalIdentifier,
            Created = DateTime.Now,
            Type = "Local"
        });

        [Fact]
        public void VerifyDefaultPropertyValues()
        {
            _provider.Name.Should().Be("Local");
            _provider.Id.Should().Be(LocalIdentifier);
            _provider.InitialPath.Should().Be(string.Empty);

            _provider.SupportsHostAuth.Should().BeFalse();
            _provider.SupportsDirectAuth.Should().BeFalse();
            _provider.SupportsOAuth.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldReturnFilesFromSpecificPath()
        {
            var real = await _provider.Get(Separator);
            var expected = Directory.GetFileSystemEntries(Separator);
            foreach (var model in real)
                expected.Should().Contain(path =>
                    model.Path == path &&
                    model.Name == Path.GetFileName(path) && 
                    model.IsFolder == File
                        .GetAttributes(path)
                        .HasFlag(FileAttributes.Directory));
        }
        
        [Fact]
        public async Task ShouldReturnDrivesFromAnEmptyPath()
        {
            var real = await _provider.Get(_provider.InitialPath);
            var expected = DriveInfo
                .GetDrives()
                .Where(p => p.DriveType != DriveType.CDRom && p.IsReady)
                .ToList();
            
            foreach (var model in real)
                expected.Should().Contain(drive =>
                    model.Name == drive.Name && model.IsFolder);
        }
    }
}