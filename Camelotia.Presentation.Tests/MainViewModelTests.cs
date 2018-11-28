using System.Linq;
using System.Threading.Tasks;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class MainViewModelTests
    {
        private readonly IProviderStorage _providerStorage = Substitute.For<IProviderStorage>();
        private readonly IFileManager _fileManager = Substitute.For<IFileManager>();
        private readonly IMainViewModel _mainViewModel;

        public MainViewModelTests() => _mainViewModel = new MainViewModel(
            (provider, files, auth) => Substitute.For<IProviderViewModel>(),
            provider => Substitute.For<IAuthViewModel>(),
            _providerStorage,
            _fileManager
        );

        [Fact]
        public async Task ShouldIndicateWhenLoadingAndReady()
        {
            _providerStorage.LoadProviders().Returns(async x =>
            {
                await Task.Delay(100);
                return Enumerable.Empty<IProvider>();
            });
            
            _mainViewModel.IsLoading.Should().BeFalse();
            _mainViewModel.IsReady.Should().BeFalse();

            _mainViewModel.LoadProviders.CanExecute(null).Should().BeTrue();
            _mainViewModel.LoadProviders.Execute(null);

            await Task.Delay(50);
            _mainViewModel.Providers.Should().BeEmpty();
            _mainViewModel.IsLoading.Should().BeTrue();
            _mainViewModel.IsReady.Should().BeFalse();

            await Task.Delay(50);
            _mainViewModel.Providers.Should().BeEmpty();
            _mainViewModel.IsLoading.Should().BeFalse();
            _mainViewModel.IsReady.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldSelectFirstProviderWhenProvidersGetLoaded()
        {
            var providers = Enumerable.Repeat(Substitute.For<IProvider>(), 1);
            _providerStorage.LoadProviders().Returns(providers);
            _mainViewModel.LoadProviders.Execute(null);

            await Task.Delay(100);
            _mainViewModel.Providers.Should().NotBeEmpty();
            _mainViewModel.SelectedProvider.Should().NotBeNull();
        }

        [Fact]
        public async Task ActivationShouldTriggerLoad()
        {
            var providers = Enumerable.Repeat(Substitute.For<IProvider>(), 1);
            _providerStorage.LoadProviders().Returns(providers);
            _mainViewModel.Providers.Should().BeEmpty();
            using (_mainViewModel.Activator.Activate())
            {
                await Task.Delay(100);
                _mainViewModel.Providers.Should().NotBeEmpty();
                _mainViewModel.SelectedProvider.Should().NotBeNull();
            }
        }
    }
}