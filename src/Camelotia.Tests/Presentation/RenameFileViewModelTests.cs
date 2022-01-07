using System;
using System.Linq;
using System.Reactive.Concurrency;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using FluentAssertions;
using NSubstitute;
using ReactiveUI;
using Xunit;

namespace Camelotia.Tests.Presentation;

public sealed class RenameFileViewModelTests
{
    private readonly ICloudViewModel _model = Substitute.For<ICloudViewModel>();
    private readonly IFileViewModel _file = Substitute.For<IFileViewModel>();
    private readonly ICloud _provider = Substitute.For<ICloud>();
    private readonly RenameFileState _state = new();

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
    public void ShouldRenameFileSuccessfullyAndCloseViewModel()
    {
        _model.CanInteract.Returns(true);
        _model.SelectedFile.Returns(_file);
        _file.Name.Returns("foo");

        var model = BuildRenameFileViewModel();
        model.OldName.Should().Be("foo");
        model.IsVisible.Should().BeFalse();
        model.Close.CanExecute().Should().BeFalse();
        model.Open.CanExecute().Should().BeTrue();

        model.Open.Execute().Subscribe();
        model.IsVisible.Should().BeTrue();
        model.Rename.CanExecute().Should().BeFalse();
        model.ErrorMessage.Should().BeNullOrEmpty();
        model.HasErrorMessage.Should().BeFalse();
        model.IsLoading.Should().BeFalse();

        model.Close.CanExecute().Should().BeTrue();
        model.Open.CanExecute().Should().BeFalse();

        model.NewName = "Foo";
        model.Rename.CanExecute().Should().BeTrue();
        model.Rename.Execute().Subscribe();

        model.IsLoading.Should().BeFalse();
        model.Rename.CanExecute().Should().BeFalse();
        model.NewName.Should().BeNullOrEmpty();
        model.OldName.Should().Be("foo");
        model.IsVisible.Should().BeFalse();

        model.Close.CanExecute().Should().BeFalse();
        model.Open.CanExecute().Should().BeTrue();
    }

    [Fact]
    public void ShouldUpdateValidationsForProperties()
    {
        _model.SelectedFile.Returns(_file);
        _file.Name.Returns("foo");

        var model = BuildRenameFileViewModel();
        model.Rename.CanExecute().Should().BeFalse();
        model.GetErrors(string.Empty).Cast<object>().Should().HaveCount(1);
        model.GetErrors(nameof(model.NewName)).Cast<object>().Should().HaveCount(1);
        model.HasErrors.Should().BeTrue();

        model.NewName = "bar";
        model.Rename.CanExecute().Should().BeTrue();
        model.GetErrors(string.Empty).Cast<object>().Should().BeEmpty();
        model.GetErrors(nameof(model.NewName)).Cast<object>().Should().BeEmpty();
        model.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void ShouldUpdateStateProperties()
    {
        const string file = "File Name";
        var model = BuildRenameFileViewModel();
        _state.NewName.Should().BeNullOrWhiteSpace();
        model.IsVisible = true;
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
