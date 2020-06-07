using System;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class DirectAuthViewModelTests
    {
        private readonly IProvider _provider = Substitute.For<IProvider>();
        private readonly TestScheduler _scheduler = new TestScheduler();
        
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
                
            _scheduler.AdvanceBy(2);
            model.HasErrors.Should().BeTrue();
            model.ErrorMessage.Should().Be("example");
        }

        [Fact]
        public void ShouldBeBusyWhenLoggingIn()
        {
            var model = BuildDirectAuthViewModel();
            model.IsBusy.Should().BeFalse();
            
            model.Username = "hello";
            model.Password = "world";
            model.Login.Execute(null);
                
            _scheduler.AdvanceBy(2);
            model.IsBusy.Should().BeTrue();
                
            _scheduler.AdvanceBy(2);
            model.IsBusy.Should().BeFalse();
        }

        private DirectAuthViewModel BuildDirectAuthViewModel()
        {
            return new DirectAuthViewModel(_provider, _scheduler, _scheduler);
        }
    }
}