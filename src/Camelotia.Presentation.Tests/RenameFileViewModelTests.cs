using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class RenameFileViewModelTests
    {
        private readonly IProviderViewModel _providerViewModel = Substitute.For<IProviderViewModel>();
        private readonly IFileViewModel _file = Substitute.For<IFileViewModel>();
        private readonly IProvider _provider = Substitute.For<IProvider>();
        private readonly TestScheduler _scheduler = new TestScheduler();

        [Fact]
        public void ShouldProperlyInitializeRenameFileViewModel() 
        {
            var model = BuildRenameFileViewModel();
            model.OldName.Should().BeNullOrEmpty();
            model.NewName.Should().BeNullOrEmpty();
            
            model.ErrorMessage.Should().BeNullOrEmpty();
            model.HasErrors.Should().BeFalse();
            model.IsVisible.Should().BeFalse();
        }

        [Fact]
        public void ShouldChangeVisibility() 
        {
            _providerViewModel.CanInteract.Returns(true);
            _providerViewModel.SelectedFile.Returns(_file);
            _file.Name.Returns("foo");

            var model = BuildRenameFileViewModel();
            _scheduler.AdvanceBy(2);
            
            model.Open.CanExecute(null).Should().BeTrue();
            model.Close.CanExecute(null).Should().BeFalse();
            model.IsVisible.Should().BeFalse();
            
            model.Open.Execute(null);
            _scheduler.AdvanceBy(2);
            model.Open.CanExecute(null).Should().BeFalse();
            model.Close.CanExecute(null).Should().BeTrue();
            model.IsVisible.Should().BeTrue();

            model.Close.Execute(null);
            _scheduler.AdvanceBy(2);
            model.Open.CanExecute(null).Should().BeTrue();
            model.Close.CanExecute(null).Should().BeFalse();
            model.IsVisible.Should().BeFalse();
        }

        [Fact]
        public void ShouldRenameFileSuccessfullyAndCloseViewModel() 
        {
            _providerViewModel.CanInteract.Returns(true);
            _providerViewModel.SelectedFile.Returns(_file);
            _file.Name.Returns("foo");
            
            var model = BuildRenameFileViewModel();
            _scheduler.AdvanceBy(2);

            model.OldName.Should().Be("foo");
            model.IsVisible.Should().BeFalse();
            model.Close.CanExecute(null).Should().BeFalse();
            model.Open.CanExecute(null).Should().BeTrue();
            model.Open.Execute(null);
            _scheduler.AdvanceBy(3);

            model.IsVisible.Should().BeTrue();
            model.Rename.CanExecute(null).Should().BeFalse();
            model.ErrorMessage.Should().BeNullOrEmpty();
            model.HasErrors.Should().BeFalse();
            model.IsLoading.Should().BeFalse();

            model.Close.CanExecute(null).Should().BeTrue();
            model.Open.CanExecute(null).Should().BeFalse();
            
            model.NewName = "Foo";
            model.Rename.CanExecute(null).Should().BeTrue();
            model.Rename.Execute(null);
            _scheduler.AdvanceBy(2);
            
            model.IsLoading.Should().BeTrue();
            model.Rename.CanExecute(null).Should().BeFalse();
            _scheduler.AdvanceBy(3);
            
            model.IsLoading.Should().BeFalse();
            model.Rename.CanExecute(null).Should().BeFalse();
            model.NewName.Should().BeNullOrEmpty();
            model.OldName.Should().Be("foo");
            model.IsVisible.Should().BeFalse();
            
            model.Close.CanExecute(null).Should().BeFalse();
            model.Open.CanExecute(null).Should().BeTrue();
        }

        private RenameFileViewModel BuildRenameFileViewModel()
        {
            return new RenameFileViewModel(_providerViewModel, _provider, _scheduler, _scheduler);
        }
    }
}