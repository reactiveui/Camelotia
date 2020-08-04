using System;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
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
        private readonly IProvider _provider = Substitute.For<IProvider>();

        [Fact]
        public void LoginCommandShouldStayDisabledUntilInputIsValid()
        {
            var model = BuildHostAuthViewModel();
            model.Login.CanExecute(null).Should().BeFalse();
            model.Address = "10.10.10.10";
            model.Username = "hello";
            model.Password = "world";
            model.Port = "5000";
            model.Login.CanExecute(null).Should().BeTrue();
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
            model.Login.Execute(null);
            
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
            model.Login.Execute(null);
            model.IsBusy.Should().BeTrue();
        }

        [Fact]
        public void ShouldTreatNonIntegersAsInvalid()
        {
            var model = BuildHostAuthViewModel();
            model.Login.CanExecute(null).Should().BeFalse();
            
            model.Port = "5000";
            model.Username = "hello";
            model.Password = "world";
            model.Address = "10.10.10.10";
            model.Login.CanExecute(null).Should().BeTrue();

            model.Port = "abc";
            model.Login.CanExecute(null).Should().BeFalse();
            
            model.Port = "42";
            model.Login.CanExecute(null).Should().BeTrue();
        }
        

        [Fact]
        public void ShouldUpdateValidationsForProperties()
        {
            var model = BuildHostAuthViewModel();
            model.Login.CanExecute(null).Should().BeFalse();
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
            
            model.Login.CanExecute(null).Should().BeTrue();
            model.GetErrors(string.Empty).Should().BeEmpty();
            model.GetErrors(nameof(model.Username)).Should().BeEmpty();
            model.GetErrors(nameof(model.Password)).Should().BeEmpty();
            model.GetErrors(nameof(model.Address)).Should().BeEmpty();
            model.GetErrors(nameof(model.Port)).Should().BeEmpty();
            model.HasErrors.Should().BeFalse();
        }

        private HostAuthViewModel BuildHostAuthViewModel()
        {
            RxApp.MainThreadScheduler = Scheduler.Immediate;
            RxApp.TaskpoolScheduler = Scheduler.Immediate;
            return new HostAuthViewModel(_provider);
        }
    }
}