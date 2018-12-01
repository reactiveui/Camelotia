using System;
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
    public sealed class OAuthViewModelTests
    {
        private readonly IProvider _provider = Substitute.For<IProvider>();

        [Fact]
        public void ShouldBeBusyWhenLoggingIn()
        {
            new TestScheduler().With(scheduler =>
            {
                var model = BuildOAuthViewModel();
                model.IsBusy.Should().BeFalse();
                model.Login.CanExecute(null).Should().BeTrue();
                model.Login.Execute(null);
                scheduler.AdvanceBy(2);
                
                model.IsBusy.Should().BeTrue();
                scheduler.AdvanceBy(2);

                model.IsBusy.Should().BeFalse();
            });
        }

        [Fact]
        public void HasErrorsShouldTriggerWhenProviderBreaks()
        {
            _provider.OAuth().Returns(x => throw new Exception("example"));
            new TestScheduler().With(scheduler =>
            {
                var model = BuildOAuthViewModel();
                model.ErrorMessage.Should().BeNullOrEmpty();
                model.HasErrors.Should().BeFalse();
            
                model.Login.CanExecute(null).Should().BeTrue();
                model.Login.Execute(null);
                scheduler.AdvanceBy(2);

                model.HasErrors.Should().BeTrue();
                model.ErrorMessage.Should().NotBeNullOrEmpty();
                model.ErrorMessage.Should().Be("example");
            });
        }
        
        private IOAuthViewModel BuildOAuthViewModel() => new OAuthViewModel(_provider);
    }
}