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
                
            model.IsAuthenticated.Should().BeFalse();
            authorized.OnNext(true);
            
            scheduler.AdvanceBy(2);    
            model.IsAuthenticated.Should().BeTrue();
        });

        [Fact]
        public void SupportsPropsShouldDependOnProvider() => new TestScheduler().With(scheduler =>
        {
            var model = BuildAuthViewModel(scheduler);
            model.SupportsDirectAuth.Should().BeFalse();
            model.SupportsOAuth.Should().BeFalse();

            _provider.SupportsDirectAuth.Returns(true);
            _provider.SupportsOAuth.Returns(true);

            model.SupportsDirectAuth.Should().BeTrue();
            model.SupportsOAuth.Should().BeTrue();
        });

        [Fact]
        public void ShouldReturnInjectedAuthViewModelTypes() => new TestScheduler().With(scheduler =>
        {
            var model = BuildAuthViewModel(scheduler);
            model.DirectAuth.Should().Be(_directAuthViewModel);
            model.OAuth.Should().Be(_oAuthViewModel);
        });

        private AuthViewModel BuildAuthViewModel(IScheduler scheduler)
        {
            return new AuthViewModel(_directAuthViewModel, _hostAuthViewModel, _oAuthViewModel, scheduler, scheduler, _provider);
        }
    }
}