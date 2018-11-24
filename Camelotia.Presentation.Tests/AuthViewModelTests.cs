using System.Reactive.Subjects;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class AuthViewModelTests
    {
        private readonly IDirectAuthViewModel _directAuthViewModel = Substitute.For<IDirectAuthViewModel>();
        private readonly IOAuthViewModel _oAuthViewModel = Substitute.For<IOAuthViewModel>();
        private readonly IProvider _provider = Substitute.For<IProvider>();

        [Fact]
        public void IsAuthenticatedPropertyShouldDependOnFileProvider()
        {
            var authorized = new Subject<bool>();
            _provider.IsAuthorized.Returns(authorized);
            
            var authViewModel = new AuthViewModel(_directAuthViewModel, _oAuthViewModel, _provider);
            authViewModel.IsAuthenticated.Should().BeFalse();

            authorized.OnNext(true);
            authViewModel.IsAuthenticated.Should().BeTrue();
        }

        [Fact]
        public void SupportsPropsShouldDependOnProvider()
        {
            var authViewModel = new AuthViewModel(_directAuthViewModel, _oAuthViewModel, _provider);
            authViewModel.SupportsDirectAuth.Should().BeFalse();
            authViewModel.SupportsOAuth.Should().BeFalse();
            
            _provider.SupportsDirectAuth.Returns(true);
            _provider.SupportsOAuth.Returns(true);
            
            authViewModel.SupportsDirectAuth.Should().BeTrue();
            authViewModel.SupportsOAuth.Should().BeTrue();
        }
    }
}