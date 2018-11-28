using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class ProviderViewModelTests
    {
        private readonly IAuthViewModel _authViewModel = Substitute.For<IAuthViewModel>();
        private readonly IFileManager _fileManager = Substitute.For<IFileManager>();
        private readonly IProvider _provider = Substitute.For<IProvider>();
        private readonly IProviderViewModel _providerViewModel;
        
        public ProviderViewModelTests()
        {
            _provider.IsAuthorized.Returns(Observable.Return(true));
            _providerViewModel = new ProviderViewModel(
                _authViewModel, _fileManager, _provider
            );
        }

        [Fact]
        public async Task ShouldLoadProvidersProper()
        {
            _provider.Get("/").Returns(async x =>
            {
                await Task.Delay(100);
                return Enumerable.Empty<FileModel>();
            });

            _providerViewModel.IsLoading.Should().BeFalse();
            _providerViewModel.IsReady.Should().BeFalse();
            
            _providerViewModel.CurrentPath.Should().Be("/");
            _providerViewModel.IsCurrentPathEmpty.Should().BeFalse();
            _providerViewModel.Files.Should().BeEmpty();

            _providerViewModel.Refresh.CanExecute(null).Should().BeTrue();
            _providerViewModel.Refresh.Execute(null);

            await Task.Delay(50);
            _providerViewModel.IsLoading.Should().BeTrue();
            _providerViewModel.IsReady.Should().BeFalse();

            await Task.Delay(50);
            _providerViewModel.IsLoading.Should().BeFalse();
            _providerViewModel.IsReady.Should().BeTrue();
            _providerViewModel.IsCurrentPathEmpty.Should().BeTrue();
        }
    }
}