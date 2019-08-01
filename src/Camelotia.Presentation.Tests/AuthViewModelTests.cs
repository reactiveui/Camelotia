using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using ReactiveUI.Testing;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class AuthViewModelTests
    {
        private readonly IDirectAuthViewModel _directAuthViewModel = Substitute.For<IDirectAuthViewModel>();
        private readonly IHostAuthViewModel _hostAuthViewModel = Substitute.For<IHostAuthViewModel>();
        private readonly IOAuthViewModel _oAuthViewModel = Substitute.For<IOAuthViewModel>();
        private readonly IProvider _provider = Substitute.For<IProvider>();

        [Fact]
        public void IsAuthenticatedPropertyShouldDependOnFileProvider() => new TestScheduler().With(scheduler =>
        {
            var authorized = new Subject<bool>();
            _provider.IsAuthorized.Returns(authorized);
            var model = BuildAuthViewModel(scheduler);

            scheduler.Start();
            model.IsAuthenticated.Should().BeFalse();
            model.IsAnonymous.Should().BeTrue();
            authorized.OnNext(true);
            
            scheduler.Start();
            model.IsAuthenticated.Should().BeTrue();
            model.IsAnonymous.Should().BeFalse();
        });

        [Fact]
        public void SupportsPropsShouldDependOnProvider() => new TestScheduler().With(scheduler =>
        {
            var model = BuildAuthViewModel(scheduler);
            model.SupportsDirectAuth.Should().BeFalse();
            model.SupportsOAuth.Should().BeFalse();
            model.SupportsHostAuth.Should().BeFalse();

            _provider.SupportsDirectAuth.ReturnsForAnyArgs(true);
            _provider.SupportsOAuth.ReturnsForAnyArgs(true);
            _provider.SupportsHostAuth.ReturnsForAnyArgs(false);

            model.SupportsDirectAuth.Should().BeTrue();
            model.SupportsOAuth.Should().BeTrue();
            model.SupportsHostAuth.Should().BeFalse();
        });

        [Fact]
        public void ShouldReturnInjectedAuthViewModelTypes() => new TestScheduler().With(scheduler =>
        {
            var model = BuildAuthViewModel(scheduler);
            model.DirectAuth.Should().Be(_directAuthViewModel);
            model.HostAuth.Should().Be(_hostAuthViewModel);
            model.OAuth.Should().Be(_oAuthViewModel);
        });

        private AuthViewModel BuildAuthViewModel(IScheduler scheduler)
        {
            return new AuthViewModel(_directAuthViewModel, _hostAuthViewModel, _oAuthViewModel, _provider, scheduler, scheduler);
        }
    }
}