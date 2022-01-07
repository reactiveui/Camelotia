using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using FluentAssertions;
using NSubstitute;
using ReactiveUI;
using Xunit;

namespace Camelotia.Tests.Presentation;

public sealed class AuthViewModelTests
{
    private readonly IDirectAuthViewModel _direct = Substitute.For<IDirectAuthViewModel>();
    private readonly IHostAuthViewModel _host = Substitute.For<IHostAuthViewModel>();
    private readonly IOAuthViewModel _open = Substitute.For<IOAuthViewModel>();
    private readonly ICloud _provider = Substitute.For<ICloud>();

    [Fact]
    public void IsAuthenticatedPropertyShouldDependOnFileProvider()
    {
        var authorized = new Subject<bool>();
        _provider.IsAuthorized.Returns(authorized);

        var model = BuildAuthViewModel();
        model.IsAuthenticated.Should().BeFalse();
        model.IsAnonymous.Should().BeTrue();

        authorized.OnNext(true);
        model.IsAuthenticated.Should().BeTrue();
        model.IsAnonymous.Should().BeFalse();
    }

    [Fact]
    public void SupportsPropsShouldDependOnProvider()
    {
        var model = BuildAuthViewModel();
        model.SupportsDirectAuth.Should().BeFalse();
        model.SupportsOAuth.Should().BeFalse();
        model.SupportsHostAuth.Should().BeFalse();

        _provider.SupportsDirectAuth.ReturnsForAnyArgs(true);
        _provider.SupportsOAuth.ReturnsForAnyArgs(true);
        _provider.SupportsHostAuth.ReturnsForAnyArgs(false);

        model.SupportsDirectAuth.Should().BeTrue();
        model.SupportsOAuth.Should().BeTrue();
        model.SupportsHostAuth.Should().BeFalse();
    }

    [Fact]
    public void ShouldReturnInjectedAuthViewModelTypes()
    {
        var model = BuildAuthViewModel();
        model.DirectAuth.Should().Be(_direct);
        model.HostAuth.Should().Be(_host);
        model.OAuth.Should().Be(_open);
    }

    private AuthViewModel BuildAuthViewModel()
    {
        RxApp.MainThreadScheduler = Scheduler.Immediate;
        RxApp.TaskpoolScheduler = Scheduler.Immediate;
        return new AuthViewModel(_direct, _host, _open, _provider);
    }
}