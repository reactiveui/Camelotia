using System;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using FluentAssertions;
using NSubstitute;
using ReactiveUI;
using Xunit;

namespace Camelotia.Presentation.Tests.ViewModels
{
    public sealed class OAuthViewModelTests
    {
        private readonly IProvider _provider = Substitute.For<IProvider>();

        [Fact]
        public void ShouldBeBusyWhenLoggingIn() 
        {
            _provider.OAuth().Returns(new Task(() => { }));
            
            var model = BuildOAuthViewModel();
            model.IsBusy.Should().BeFalse();
            model.Login.CanExecute(null).Should().BeTrue();
            model.Login.Execute(null);
            
            model.IsBusy.Should().BeTrue();
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

            model.HasErrors.Should().BeTrue();
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