using System.IO;
using System.Reactive.Concurrency;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using FluentAssertions;
using NSubstitute;
using ReactiveUI;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class CreateFolderViewModelTests
    {
        private static readonly string Separator = Path.DirectorySeparatorChar.ToString();
        private readonly IProviderViewModel _providerViewModel = Substitute.For<IProviderViewModel>();
        private readonly IProvider _provider = Substitute.For<IProvider>();
        
        public CreateFolderViewModelTests()
        {
            RxApp.MainThreadScheduler = Scheduler.Immediate;
            RxApp.TaskpoolScheduler = Scheduler.Immediate;
        }

        [Fact]
        public void ShouldProperlyInitializeCreateFolderViewModel()
        {
            var model = BuildCreateFolderViewModel();
            model.Name.Should().BeNullOrEmpty();
            model.Path.Should().BeNullOrEmpty();

            model.ErrorMessage.Should().BeNullOrEmpty();
            model.HasErrors.Should().BeFalse();
            model.IsVisible.Should().BeFalse();
        }

        [Fact]
        public void ShouldChangeVisibility()
        {
            _providerViewModel.CanInteract.Returns(true);
            _providerViewModel.CurrentPath.Returns(Separator);
            _provider.CanCreateFolder.Returns(true);
            
            var model = BuildCreateFolderViewModel();
            model.Open.CanExecute(null).Should().BeTrue();
            model.Close.CanExecute(null).Should().BeFalse();
            model.IsVisible.Should().BeFalse();
            model.Open.Execute(null);
            
            model.Open.CanExecute(null).Should().BeFalse();
            model.Close.CanExecute(null).Should().BeTrue();
            model.IsVisible.Should().BeTrue();
            model.Close.Execute(null);
            
            model.Open.CanExecute(null).Should().BeTrue();
            model.Close.CanExecute(null).Should().BeFalse();
            model.IsVisible.Should().BeFalse();
        }

        [Fact]
        public void ShouldCreateFolderSuccessfullyAndCloseViewModel()
        {
            _providerViewModel.CanInteract.Returns(true);
            _providerViewModel.CurrentPath.Returns(Separator);
            _provider.CanCreateFolder.Returns(true);

            var model = BuildCreateFolderViewModel();
            model.IsVisible.Should().BeFalse();
            model.Close.CanExecute(null).Should().BeFalse();
            model.Open.CanExecute(null).Should().BeTrue();
            model.Open.Execute(null);

            model.IsVisible.Should().BeTrue();
            model.Create.CanExecute(null).Should().BeFalse();
            model.ErrorMessage.Should().BeNullOrEmpty();
            model.HasErrors.Should().BeFalse();
            model.IsLoading.Should().BeFalse();

            model.Close.CanExecute(null).Should().BeTrue();
            model.Open.CanExecute(null).Should().BeFalse();
            
            model.Name = "Foo";
            model.Create.CanExecute(null).Should().BeTrue();
            model.Create.Execute(null);
            
            model.IsLoading.Should().BeFalse();
            model.Create.CanExecute(null).Should().BeFalse();
            model.Name.Should().BeNullOrEmpty();
            model.Path.Should().Be(Separator);
            model.IsVisible.Should().BeFalse();
            
            model.Close.CanExecute(null).Should().BeFalse();
            model.Open.CanExecute(null).Should().BeTrue();
        }

        private CreateFolderViewModel BuildCreateFolderViewModel()
        {
            return new CreateFolderViewModel(_providerViewModel, _provider);
        }
    }
}