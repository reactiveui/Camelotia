using System;
using System.Reactive.Concurrency;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using ReactiveUI.Testing;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class DirectAuthViewModelTests
    {
        private readonly IProvider _provider = Substitute.For<IProvider>();
        
        [Fact]
        public void LoginCommandShouldStayDisabledUntilInputIsValid() => new TestScheduler().With(scheduler =>
        {
            var model = BuildDirectAuthViewModel(scheduler);
            model.Login.CanExecute(null).Should().BeFalse();
            model.Username = "hello";
            model.Password = "world";
            model.Login.CanExecute(null).Should().BeTrue();
        });

        [Fact]
        public void HasErrorsShouldTriggerWhenProviderBreaks() => new TestScheduler().With(scheduler =>
        {
            _provider
                .DirectAuth("hello", "world")
                .Returns(x => throw new Exception("example"));
                
            var model = BuildDirectAuthViewModel(scheduler);
            model.HasErrors.Should().BeFalse();
                
            model.Username = "hello";
            model.Password = "world";
            model.Login.Execute(null);
                
            scheduler.AdvanceBy(2);
            model.HasErrors.Should().BeTrue();
            model.ErrorMessage.Should().Be("example");
        });

        [Fact]
        public void ShouldBeBusyWhenLoggingIn() => new TestScheduler().With(scheduler =>
        {
            var model = BuildDirectAuthViewModel(scheduler);
            model.IsBusy.Should().BeFalse();
            
            model.Username = "hello";
            model.Password = "world";
            model.Login.Execute(null);
                
            scheduler.AdvanceBy(2);
            model.IsBusy.Should().BeTrue();
                
            scheduler.AdvanceBy(2);
            model.IsBusy.Should().BeFalse();
        });

        private DirectAuthViewModel BuildDirectAuthViewModel(IScheduler scheduler)
        {
            return new DirectAuthViewModel(_provider, scheduler, scheduler);
        }
    }
}