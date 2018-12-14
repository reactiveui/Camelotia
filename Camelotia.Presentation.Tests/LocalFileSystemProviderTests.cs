using System.IO;
using System.Threading.Tasks;
using Camelotia.Services.Models;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Providers;
using FluentAssertions;
using Xunit;

namespace Camelotia.Services.Tests
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
        [Theory]
        [InlineData(0, "0B")]
        [InlineData(520400, "520.4KB")]
        [InlineData(520040000, "520MB")]
        [InlineData(520068000, "520.1MB")]
        [InlineData(520185000000, "520.2GB")]
        public void ByteConverterShouldCalculateWithNoPrecisionSupplied(long byteCount, string expectedValue)
        {
            var stringValue = ByteConverter.BytesToString(byteCount);
            stringValue.Should().Be(expectedValue);
        }
        [Theory]
        [InlineData(115, 1, "115B")]
        [InlineData(115, 3, "115B")]
        [InlineData(520348, 3, "520.348KB")]
        [InlineData(520462400, 3, "520.462MB")]
        [InlineData(520573990000, 3, "520.574GB")]
        [InlineData(520124960000, 3, "520.125GB")]
        public void ByteConverterShouldCalculate(long byteCount, int precision, string expectedValue)
        {
            var stringValue = ByteConverter.BytesToString(byteCount, precision);
            stringValue.Should().Be(expectedValue);
        }
    }
}