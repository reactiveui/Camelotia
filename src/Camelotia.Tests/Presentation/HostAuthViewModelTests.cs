using System;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using FluentAssertions;
using NSubstitute;
using ReactiveUI;
using Xunit;

namespace Camelotia.Tests.Presentation
{
    public sealed class HostAuthViewModelTests
    {
        private readonly ICloud _provider = Substitute.For<ICloud>();
        private readonly HostAuthState _state = new HostAuthState();

        [Fact]
        public void LoginCommandShouldStayDisabledUntilInputIsValid()
        {
            var model = BuildHostAuthViewModel();
            model.Login.CanExecute().Should().BeFalse();
            model.Address = "10.10.10.10";
            model.Username = "hello";
            model.Password = "world";
            model.Port = "5000";
            model.Login.CanExecute().Should().BeTrue();
        }

        [Fact]
        public void HasErrorMessageShouldTriggerWhenProviderBreaks()
        {
            _provider.HostAuth("10.10.10.10", 5000, "hello", "world").Returns(x => throw new Exception("example"));

            var model = BuildHostAuthViewModel();
            model.HasErrorMessage.Should().BeFalse();

            model.Port = "5000";
            model.Username = "hello";
            model.Password = "world";
            model.Address = "10.10.10.10";
            model.Login.Execute().Subscribe(ok => { }, error => { });

            model.HasErrorMessage.Should().BeTrue();
            model.ErrorMessage.Should().Be("example");
        }

        [Fact]
        public void ShouldBeBusyWhenLoggingIn()
        {
            _provider.HostAuth("10.10.10.10", 5000, "hello", "world").Returns(new Task(() => { }));

            var model = BuildHostAuthViewModel();
            model.IsBusy.Should().BeFalse();

            model.Port = "5000";
            model.Username = "hello";
            model.Password = "world";
            model.Address = "10.10.10.10";
            model.Login.Execute().Subscribe();
            model.IsBusy.Should().BeTrue();
        }

        [Fact]
        public void ShouldTreatNonIntegersAsInvalid()
        {
            var model = BuildHostAuthViewModel();
            model.Login.CanExecute().Should().BeFalse();

            model.Port = "5000";
            model.Username = "hello";
            model.Password = "world";
            model.Address = "10.10.10.10";
            model.Login.CanExecute().Should().BeTrue();

            model.Port = "abc";
            model.Login.CanExecute().Should().BeFalse();

            model.Port = "42";
            model.Login.CanExecute().Should().BeTrue();
        }

        [Fact]
        public void ShouldUpdateValidationsForProperties()
        {
            var model = BuildHostAuthViewModel();
            model.Login.CanExecute().Should().BeFalse();
            model.GetErrors(string.Empty).Should().HaveCount(4);
            model.GetErrors(nameof(model.Username)).Should().NotBeEmpty();
            model.GetErrors(nameof(model.Password)).Should().NotBeEmpty();
            model.GetErrors(nameof(model.Address)).Should().NotBeEmpty();
            model.GetErrors(nameof(model.Port)).Should().NotBeEmpty();
            model.HasErrors.Should().BeTrue();

            model.Username = "Jotaro";
            model.Password = "qwerty";
            model.Address = "127.0.0.1";
            model.Port = "5000";

            model.Login.CanExecute().Should().BeTrue();
            model.GetErrors(string.Empty).Should().BeEmpty();
            model.GetErrors(nameof(model.Username)).Should().BeEmpty();
            model.GetErrors(nameof(model.Password)).Should().BeEmpty();
            model.GetErrors(nameof(model.Address)).Should().BeEmpty();
            model.GetErrors(nameof(model.Port)).Should().BeEmpty();
            model.HasErrors.Should().BeFalse();
        }

        [Fact]
        public void ShouldUpdateStateProperties()
        {
            const string user = "Joseph";
            const string pass = "qwerty";
            const string address = "127.0.0.1";
            const string port = "42";

            var model = BuildHostAuthViewModel();
            _state.Username.Should().BeNullOrWhiteSpace();
            _state.Password.Should().BeNullOrWhiteSpace();
            _state.Address.Should().BeNullOrWhiteSpace();
            _state.Port.Should().BeNullOrWhiteSpace();

            model.Username = user;
            model.Password = pass;
            model.Address = address;
            model.Port = port;

            _state.Username.Should().Be(user);
            _state.Password.Should().Be(pass);
            _state.Address.Should().Be(address);
            _state.Port.Should().Be(port);
        }

        private HostAuthViewModel BuildHostAuthViewModel()
        {
            RxApp.MainThreadScheduler = Scheduler.Immediate;
            RxApp.TaskpoolScheduler = Scheduler.Immediate;
            return new HostAuthViewModel(_state, _provider);
        }
    }
}