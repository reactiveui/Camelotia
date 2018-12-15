using Camelotia.Services.Models;
using FluentAssertions;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class ByteConverterTests
    {
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