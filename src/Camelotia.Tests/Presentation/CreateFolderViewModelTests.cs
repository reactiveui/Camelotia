using System;
using System.IO;
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
    public sealed class CreateFolderViewModelTests
    {
        private static readonly string Separator = Path.DirectorySeparatorChar.ToString();
        private readonly ICloudViewModel _model = Substitute.For<ICloudViewModel>();
        private readonly ICloud _provider = Substitute.For<ICloud>();
        private readonly CreateFolderState _state = new CreateFolderState();

        [Fact]
        public void ShouldProperlyInitializeCreateFolderViewModel()
        {
            var model = BuildCreateFolderViewModel();
            model.Name.Should().BeNullOrEmpty();
            model.Path.Should().BeNullOrEmpty();

            model.ErrorMessage.Should().BeNullOrEmpty();
            model.HasErrorMessage.Should().BeFalse();
            model.IsVisible.Should().BeFalse();
        }

        [Fact]
        public void ShouldChangeVisibility()
        {
            _model.CanInteract.Returns(true);
            _model.CurrentPath.Returns(Separator);
            _provider.CanCreateFolder.Returns(true);

            var model = BuildCreateFolderViewModel();
            model.Open.CanExecute().Should().BeTrue();
            model.Open.CanExecute().Should().BeTrue();
            model.Close.CanExecute().Should().BeFalse();
            model.IsVisible.Should().BeFalse();
            model.Open.Execute().Subscribe();

            model.Open.CanExecute().Should().BeFalse();
            model.Close.CanExecute().Should().BeTrue();
            model.IsVisible.Should().BeTrue();
            model.Close.Execute().Subscribe();

            model.Open.CanExecute().Should().BeTrue();
            model.Close.CanExecute().Should().BeFalse();
            model.IsVisible.Should().BeFalse();
        }

        [Fact]
        public void ShouldCreateFolderSuccessfullyAndCloseViewModel()
        {
            _model.CanInteract.Returns(true);
            _model.CurrentPath.Returns(Separator);
            _provider.CanCreateFolder.Returns(true);

            var model = BuildCreateFolderViewModel();
            model.IsVisible.Should().BeFalse();
            model.Close.CanExecute().Should().BeFalse();
            model.Open.CanExecute().Should().BeTrue();
            model.Open.Execute().Subscribe();

            model.IsVisible.Should().BeTrue();
            model.Create.CanExecute().Should().BeFalse();
            model.ErrorMessage.Should().BeNullOrEmpty();
            model.HasErrorMessage.Should().BeFalse();
            model.IsLoading.Should().BeFalse();

            model.Close.CanExecute().Should().BeTrue();
            model.Open.CanExecute().Should().BeFalse();

            model.Name = "Foo";
            model.Create.CanExecute().Should().BeTrue();
            model.Create.Execute().Subscribe();

            model.IsLoading.Should().BeFalse();
            model.Create.CanExecute().Should().BeFalse();
            model.Name.Should().BeNullOrEmpty();
            model.Path.Should().Be(Separator);
            model.IsVisible.Should().BeFalse();

            model.Close.CanExecute().Should().BeFalse();
            model.Open.CanExecute().Should().BeTrue();
        }

        [Fact]
        public void ShouldUpdateValidationsForProperties()
        {
            _model.CurrentPath.Returns(Separator);

            var model = BuildCreateFolderViewModel();
            model.Create.CanExecute().Should().BeFalse();
            model.GetErrors(string.Empty).Should().HaveCount(1);
            model.GetErrors(nameof(model.Name)).Should().HaveCount(1);
            model.HasErrors.Should().BeTrue();

            model.Name = "Example";
            model.Create.CanExecute().Should().BeTrue();
            model.GetErrors(string.Empty).Should().BeEmpty();
            model.GetErrors(nameof(model.Name)).Should().BeEmpty();
            model.HasErrors.Should().BeFalse();
        }

        [Fact]
        public void ShouldUpdateStateProperties()
        {
            const string name = "Secret Folder";
            var model = BuildCreateFolderViewModel();

            _state.Name.Should().BeNullOrWhiteSpace();
            _state.IsVisible.Should().BeFalse();

            model.Name = name;
            model.IsVisible = true;

            _state.Name.Should().Be(name);
            _state.IsVisible.Should().BeTrue();
        }

        private CreateFolderViewModel BuildCreateFolderViewModel()
        {
            RxApp.MainThreadScheduler = Scheduler.Immediate;
            RxApp.TaskpoolScheduler = Scheduler.Immediate;
            return new CreateFolderViewModel(_state, _model, _provider);
        }
    }
}