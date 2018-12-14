using System.IO;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Providers;
using FluentAssertions;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class LocalFileSystemProviderTests
    {
        private static readonly string Separator = Path.DirectorySeparatorChar.ToString();
        private readonly IProvider _provider = new LocalFileSystemProvider();
        
        [Fact]
        public async Task LocalFileSystemShouldNotSupportAuth()
        {
            _provider.SupportsDirectAuth.Should().BeFalse();
            _provider.SupportsOAuth.Should().BeFalse();
            await _provider.DirectAuth(string.Empty, string.Empty);
            await _provider.OAuth();
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
    }
}