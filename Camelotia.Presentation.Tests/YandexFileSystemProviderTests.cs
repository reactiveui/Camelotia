using Camelotia.Services.Interfaces;
using Camelotia.Services.Providers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class YandexFileSystemProviderTests
    {
        private readonly IUriLauncher _uriLauncher = Substitute.For<IUriLauncher>();
        private readonly IListener _listener = Substitute.For<IListener>();
        private readonly IProvider _provider;

        public YandexFileSystemProviderTests() => _provider = new YandexFileSystemProvider(_uriLauncher, _listener);

        [Fact]
        public void ShouldImplementNonNullInitialPath() => _provider.InitialPath.Should().NotBeNull();
    }
}