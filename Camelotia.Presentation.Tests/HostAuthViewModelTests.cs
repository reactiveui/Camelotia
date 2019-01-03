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
    public sealed class HostAuthViewModelTests
    {
        private readonly IProvider _provider = Substitute.For<IProvider>();
        
        [Fact]
        public void LoginCommandShouldStayDisabledUntilInputIsValid() => new TestScheduler().With(scheduler =>
        {
            var model = BuildHostAuthViewModel(scheduler);
            model.Login.CanExecute(null).Should().BeFalse();
            model.Address = "10.10.10.10";
            model.Username = "hello";
            model.Password = "world";
            model.Port = "5000";
            model.Login.CanExecute(null).Should().BeTrue();
        });

        [Fact]
        public void HasErrorsShouldTriggerWhenProviderBreaks() => new TestScheduler().With(scheduler =>
        {
            _provider
                .HostAuth("10.10.10.10", 5000, "hello", "world")
                .Returns(x => throw new Exception("example"));
                
            var model = BuildHostAuthViewModel(scheduler);
            model.HasErrors.Should().BeFalse();
                
            model.Port = "5000";
            model.Username = "hello";
            model.Password = "world";
            model.Address = "10.10.10.10";
            model.Login.Execute(null);
                
            scheduler.AdvanceBy(2);
            model.HasErrors.Should().BeTrue();
            model.ErrorMessage.Should().Be("example");
        });

        [Fact]
        public void ShouldBeBusyWhenLoggingIn() => new TestScheduler().With(scheduler =>
        {
            var model = BuildHostAuthViewModel(scheduler);
            model.IsBusy.Should().BeFalse();

            model.Port = "5000";
            model.Username = "hello";
            model.Password = "world";
            model.Address = "10.10.10.10";
            model.Login.Execute(null);
                
            scheduler.AdvanceBy(2);
            model.IsBusy.Should().BeTrue();
                
            scheduler.AdvanceBy(2);
            model.IsBusy.Should().BeFalse();
        });

        [Fact]
        public void ShouldTreatNonIntegersAsInvalid() => new TestScheduler().With(scheduler =>
        {
            var model = BuildHostAuthViewModel(scheduler);
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
        });

        private HostAuthViewModel BuildHostAuthViewModel(IScheduler scheduler)
        {
            return new HostAuthViewModel(scheduler, scheduler, _provider);
        }
    }
}