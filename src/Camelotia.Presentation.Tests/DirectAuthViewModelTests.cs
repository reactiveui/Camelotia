using System;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using FluentAssertions;
using NSubstitute;
using ReactiveUI;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class DirectAuthViewModelTests
    {
        private readonly IProvider _provider = Substitute.For<IProvider>();
        
        public DirectAuthViewModelTests()
        {
            RxApp.MainThreadScheduler = Scheduler.Immediate;
            RxApp.TaskpoolScheduler = Scheduler.Immediate;
        }

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
        public void HasErrorsShouldTriggerWhenProviderBreaks()
        {
            _provider
                .DirectAuth("hello", "world")
                .Returns(x => throw new Exception("example"));
                
            var model = BuildDirectAuthViewModel();
            model.HasErrors.Should().BeFalse();
                
            model.Username = "hello";
            model.Password = "world";
            model.Login.Execute(null);
            model.HasErrors.Should().BeTrue();
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

        private DirectAuthViewModel BuildDirectAuthViewModel()
        {
            return new DirectAuthViewModel(_provider);
        }
    }
}