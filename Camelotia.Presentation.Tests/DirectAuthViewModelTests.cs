using System;
using System.Threading.Tasks;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class DirectAuthViewModelTests
    {
        private readonly IProvider _provider = Substitute.For<IProvider>();
        private readonly IDirectAuthViewModel _directAuthViewModel;

        public DirectAuthViewModelTests() => _directAuthViewModel = new DirectAuthViewModel(_provider);
        
        [Fact]
        public void LoginCommandShouldStayDisabledUntilInputIsValid()
        {
            _directAuthViewModel.Login.CanExecute(null).Should().BeFalse();

            _directAuthViewModel.Username = "hello";
            _directAuthViewModel.Password = "world";
            _directAuthViewModel.Login.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void HasErrorsShouldTriggerWhenProviderBreaks()
        {
            _provider.DirectAuth("hello", "world").Returns(x => throw new Exception("example"));
            _directAuthViewModel.HasErrors.Should().BeFalse();

            _directAuthViewModel.Username = "hello";
            _directAuthViewModel.Password = "world";
            _directAuthViewModel.Login.Execute(null);

            _directAuthViewModel.HasErrors.Should().BeTrue();
            _directAuthViewModel.ErrorMessage.Should().Be("example");
        }

        [Fact]
        public async Task ShouldBeBusyWhenLoggingIn()
        {
            _provider.DirectAuth("hello", "world").Returns(x => Task.Delay(100));
            _directAuthViewModel.IsBusy.Should().BeFalse();
            
            _directAuthViewModel.Username = "hello";
            _directAuthViewModel.Password = "world";
            _directAuthViewModel.Login.Execute(null);
            
            await Task.Delay(50);
            _directAuthViewModel.IsBusy.Should().BeTrue();

            await Task.Delay(100);
            _directAuthViewModel.IsBusy.Should().BeFalse();
        }
    }
}