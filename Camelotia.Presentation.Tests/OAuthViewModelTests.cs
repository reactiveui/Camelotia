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
    public sealed class OAuthViewModelTests
    {
        private readonly IProvider _provider = Substitute.For<IProvider>();
        private readonly IOAuthViewModel _authViewModel;

        public OAuthViewModelTests() => _authViewModel = new OAuthViewModel(_provider);

        [Fact]
        public async Task ShouldBeBusyWhenLoggingIn()
        {
            _provider.OAuth().Returns(x => Task.Delay(200));
            _authViewModel.IsBusy.Should().BeFalse();

            _authViewModel.Login.CanExecute(null).Should().BeTrue();
            _authViewModel.Login.Execute(null);

            await Task.Delay(100);
            _authViewModel.IsBusy.Should().BeTrue();

            await Task.Delay(100);
            _authViewModel.IsBusy.Should().BeFalse();
        }

        [Fact]
        public void HasErrorsShouldTriggerWhenProviderBreaks()
        {
            _provider.OAuth().Returns(x => throw new Exception("example"));
            _authViewModel.ErrorMessage.Should().BeNullOrEmpty();
            _authViewModel.HasErrors.Should().BeFalse();
            
            _authViewModel.Login.CanExecute(null).Should().BeTrue();
            _authViewModel.Login.Execute(null);

            _authViewModel.HasErrors.Should().BeTrue();
            _authViewModel.ErrorMessage.Should().NotBeNullOrEmpty();
            _authViewModel.ErrorMessage.Should().Be("example");
        }
    }
}