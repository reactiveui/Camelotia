using System;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class OAuthViewModelTests
    {
        private readonly IProvider _provider = Substitute.For<IProvider>();
        private readonly TestScheduler _scheduler = new TestScheduler();

        [Fact]
        public void ShouldBeBusyWhenLoggingIn() 
        {
            var model = BuildOAuthViewModel();
            model.IsBusy.Should().BeFalse();
            model.Login.CanExecute(null).Should().BeTrue();
            model.Login.Execute(null);
            
            _scheduler.AdvanceBy(2);    
            model.IsBusy.Should().BeTrue();
            
            _scheduler.AdvanceBy(2);
            model.IsBusy.Should().BeFalse();
        }

        [Fact]
        public void HasErrorsShouldTriggerWhenProviderBreaks() 
        {
            _provider.OAuth().Returns(x => throw new Exception("example"));
            
            var model = BuildOAuthViewModel();    
            model.ErrorMessage.Should().BeNullOrEmpty();
            model.HasErrors.Should().BeFalse();
            
            model.Login.CanExecute(null).Should().BeTrue();
            model.Login.Execute(null);
            _scheduler.AdvanceBy(2);

            model.HasErrors.Should().BeTrue();
            model.ErrorMessage.Should().NotBeNullOrEmpty();
            model.ErrorMessage.Should().Be("example");
        }

        private OAuthViewModel BuildOAuthViewModel()
        {
            return new OAuthViewModel(_provider, _scheduler, _scheduler);
        }
    }
}