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
    public sealed class OAuthViewModelTests
    {
        private readonly ICloud _provider = Substitute.For<ICloud>();

        [Fact]
        public void ShouldBeBusyWhenLoggingIn() 
        {
            _provider.OAuth().Returns(new Task(() => { }));
            
            var model = BuildOAuthViewModel();
            model.IsBusy.Should().BeFalse();
            model.Login.CanExecute().Should().BeTrue();
            model.Login.Execute().Subscribe();
            
            model.IsBusy.Should().BeTrue();
        }

        [Fact]
        public void HasErrorMessageShouldTriggerWhenProviderBreaks() 
        {
            _provider.OAuth().Returns(x => throw new Exception("example"));
            
            var model = BuildOAuthViewModel();    
            model.ErrorMessage.Should().BeNullOrEmpty();
            model.HasErrorMessage.Should().BeFalse();
            
            model.Login.CanExecute().Should().BeTrue();
            model.Login.Execute().Subscribe(ok => { }, err => { });

            model.HasErrorMessage.Should().BeTrue();
            model.ErrorMessage.Should().NotBeNullOrEmpty();
            model.ErrorMessage.Should().Be("example");
        }

        private OAuthViewModel BuildOAuthViewModel()
        {
            RxApp.MainThreadScheduler = Scheduler.Immediate;
            RxApp.TaskpoolScheduler = Scheduler.Immediate;
            return new OAuthViewModel(_provider);
        }
    }
}