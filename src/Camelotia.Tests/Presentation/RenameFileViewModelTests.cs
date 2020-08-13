using System.Reactive.Concurrency;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using FluentAssertions;
using NSubstitute;
using ReactiveUI;
using Xunit;

namespace Camelotia.Tests.Presentation
{
    public sealed class RenameFileViewModelTests
    {
        private readonly IProviderViewModel _model = Substitute.For<IProviderViewModel>();
        private readonly IFileViewModel _file = Substitute.For<IFileViewModel>();
        private readonly IProvider _provider = Substitute.For<IProvider>();
        private readonly RenameFileState _state = new RenameFileState();

        [Fact]
        public void ShouldProperlyInitializeRenameFileViewModel() 
        {
            var model = BuildRenameFileViewModel();
            model.OldName.Should().BeNullOrEmpty();
            model.NewName.Should().BeNullOrEmpty();
            
            model.ErrorMessage.Should().BeNullOrEmpty();
            model.HasErrorMessage.Should().BeFalse();
            model.IsVisible.Should().BeFalse();
        }

        [Fact]
        public void ShouldChangeVisibility() 
        {
            _model.CanInteract.Returns(true);
            _model.SelectedFile.Returns(_file);
            _file.Name.Returns("foo");

            var model = BuildRenameFileViewModel();
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
        public void ShouldRenameFileSuccessfullyAndCloseViewModel() 
        {
            _model.CanInteract.Returns(true);
            _model.SelectedFile.Returns(_file);
            _file.Name.Returns("foo");
            
            var model = BuildRenameFileViewModel();
            model.OldName.Should().Be("foo");
            model.IsVisible.Should().BeFalse();
            model.Close.CanExecute(null).Should().BeFalse();
            model.Open.CanExecute(null).Should().BeTrue();

            model.Open.Execute(null);
            model.IsVisible.Should().BeTrue();
            model.Rename.CanExecute(null).Should().BeFalse();
            model.ErrorMessage.Should().BeNullOrEmpty();
            model.HasErrorMessage.Should().BeFalse();
            model.IsLoading.Should().BeFalse();

            model.Close.CanExecute(null).Should().BeTrue();
            model.Open.CanExecute(null).Should().BeFalse();
            
            model.NewName = "Foo";
            model.Rename.CanExecute(null).Should().BeTrue();
            model.Rename.Execute(null);
            
            model.IsLoading.Should().BeFalse();
            model.Rename.CanExecute(null).Should().BeFalse();
            model.NewName.Should().BeNullOrEmpty();
            model.OldName.Should().Be("foo");
            model.IsVisible.Should().BeFalse();
            
            model.Close.CanExecute(null).Should().BeFalse();
            model.Open.CanExecute(null).Should().BeTrue();
        }
        
        [Fact]
        public void ShouldUpdateValidationsForProperties()
        {
            _model.SelectedFile.Returns(_file);
            _file.Name.Returns("foo");
            
            var model = BuildRenameFileViewModel();
            model.Rename.CanExecute(null).Should().BeFalse();
            model.GetErrors(string.Empty).Should().HaveCount(1);
            model.GetErrors(nameof(model.NewName)).Should().HaveCount(1);
            model.HasErrors.Should().BeTrue();

            model.NewName = "bar";
            model.Rename.CanExecute(null).Should().BeTrue();
            model.GetErrors(string.Empty).Should().BeEmpty();
            model.GetErrors(nameof(model.NewName)).Should().BeEmpty();
            model.HasErrors.Should().BeFalse();
        }

        [Fact]
        public void ShouldUpdateStateProperties()
        {
            const string file = "File Name";
            var model = BuildRenameFileViewModel();
            _state.NewName.Should().BeNullOrWhiteSpace();

            model.NewName = file;
            _state.NewName.Should().Be(file);
        }

        private RenameFileViewModel BuildRenameFileViewModel()
        {
            RxApp.MainThreadScheduler = Scheduler.Immediate;
            RxApp.TaskpoolScheduler = Scheduler.Immediate;
            return new RenameFileViewModel(_state, _model, _provider);
        }
    }
}