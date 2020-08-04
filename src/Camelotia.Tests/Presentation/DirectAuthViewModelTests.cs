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
    public sealed class DirectAuthViewModelTests
    {
        private readonly IProvider _provider = Substitute.For<IProvider>();

        [Fact]
        public void LoginCommandShouldStayDisabledUntilInputIsValid()
        {
            var model = BuildDirectAuthViewModel();
            model.Login.CanExecute(null).Should().BeFalse();
            model.Username = "hello";
            model.Password = "world";
            model.Login.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void HasErrorMessageShouldTriggerWhenProviderBreaks()
        {
            _provider.DirectAuth("hello", "world").Returns(x => throw new Exception("example"));
            
            var model = BuildDirectAuthViewModel();
            model.HasErrorMessage.Should().BeFalse();
                
            model.Username = "hello";
            model.Password = "world";
            model.Login.Execute(null);
            model.HasErrorMessage.Should().BeTrue();
            model.ErrorMessage.Should().Be("example");
        }

        [Fact]
        public void ShouldBeBusyWhenLoggingIn()
        {
            _provider.DirectAuth("hello", "world").Returns(new Task(() => { }));
            
            var model = BuildDirectAuthViewModel();
            model.IsBusy.Should().BeFalse();
            
            model.Username = "hello";
            model.Password = "world";
            model.Login.Execute(null);
            model.IsBusy.Should().BeTrue();
        }

        [Fact]
        public void ShouldUpdateValidationsForProperties()
        {
            var model = BuildDirectAuthViewModel();
            model.Login.CanExecute(null).Should().BeFalse();
            model.GetErrors(string.Empty).Should().HaveCount(2);
            model.GetErrors(nameof(model.Username)).Should().NotBeEmpty();
            model.GetErrors(nameof(model.Password)).Should().NotBeEmpty();
            model.HasErrors.Should().BeTrue();

            model.Username = "Jotaro";
            model.Login.CanExecute(null).Should().BeFalse();
            model.GetErrors(string.Empty).Should().HaveCount(1);
            model.GetErrors(nameof(model.Username)).Should().BeEmpty();
            model.GetErrors(nameof(model.Password)).Should().NotBeEmpty();
            model.HasErrors.Should().BeTrue();

            model.Password = "qwerty";
            model.Login.CanExecute(null).Should().BeTrue();
            model.GetErrors(string.Empty).Should().BeEmpty();
            model.GetErrors(nameof(model.Username)).Should().BeEmpty();
            model.GetErrors(nameof(model.Password)).Should().BeEmpty();
            model.HasErrors.Should().BeFalse();
        }

        private DirectAuthViewModel BuildDirectAuthViewModel()
        {
            RxApp.MainThreadScheduler = Scheduler.Immediate;
            RxApp.TaskpoolScheduler = Scheduler.Immediate;
            return new DirectAuthViewModel(_provider);
        }
    }
}