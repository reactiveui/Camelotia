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
    public sealed class OAuthViewModelTests
    {
        private readonly IProvider _provider = Substitute.For<IProvider>();

        [Fact]
        public void ShouldBeBusyWhenLoggingIn() => new TestScheduler().With(scheduler =>
        {
            var model = BuildOAuthViewModel(scheduler);
            model.IsBusy.Should().BeFalse();
            model.Login.CanExecute(null).Should().BeTrue();
            model.Login.Execute(null);
            
            scheduler.AdvanceBy(2);    
            model.IsBusy.Should().BeTrue();
            
            scheduler.AdvanceBy(2);
            model.IsBusy.Should().BeFalse();
        });

        [Fact]
        public void HasErrorsShouldTriggerWhenProviderBreaks() => new TestScheduler().With(scheduler =>
        {
            _provider.OAuth().Returns(x => throw new Exception("example"));
            
            var model = BuildOAuthViewModel(scheduler);    
            model.ErrorMessage.Should().BeNullOrEmpty();
            model.HasErrors.Should().BeFalse();
            
            model.Login.CanExecute(null).Should().BeTrue();
            model.Login.Execute(null);
            scheduler.AdvanceBy(2);

            model.HasErrors.Should().BeTrue();
            model.ErrorMessage.Should().NotBeNullOrEmpty();
            model.ErrorMessage.Should().Be("example");
        });

        private OAuthViewModel BuildOAuthViewModel(IScheduler scheduler)
        {
            return new OAuthViewModel(_provider, scheduler, scheduler);
        }
    }
}