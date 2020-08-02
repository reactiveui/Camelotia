using System.Reactive.Concurrency;
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
            model.HasErrors.Should().BeFalse();
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

        private RenameFileViewModel BuildRenameFileViewModel()
        {
            RxApp.MainThreadScheduler = Scheduler.Immediate;
            RxApp.TaskpoolScheduler = Scheduler.Immediate;
            return new RenameFileViewModel(_model, _provider);
        }
    }
}