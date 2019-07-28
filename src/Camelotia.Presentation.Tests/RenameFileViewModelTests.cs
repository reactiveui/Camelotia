using System.Reactive.Concurrency;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using ReactiveUI.Testing;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class RenameFileViewModelTests
    {
        private readonly IProviderViewModel _providerViewModel = Substitute.For<IProviderViewModel>();
        private readonly IFileViewModel _file = Substitute.For<IFileViewModel>();
        private readonly IProvider _provider = Substitute.For<IProvider>();

        [Fact]
        public void ShouldProperlyInitializeRenameFileViewModel() => new TestScheduler().With(scheduler =>
        {
            var model = BuildRenameFileViewModel(scheduler);
            model.OldName.Should().BeNullOrEmpty();
            model.NewName.Should().BeNullOrEmpty();
            
            model.ErrorMessage.Should().BeNullOrEmpty();
            model.HasErrors.Should().BeFalse();
            model.IsVisible.Should().BeFalse();
        });

        [Fact]
        public void ShouldChangeVisibility() => new TestScheduler().With(scheduler =>
        {
            _providerViewModel.CanInteract.Returns(true);
            _providerViewModel.SelectedFile.Returns(_file);
            _file.Name.Returns("foo");

            var model = BuildRenameFileViewModel(scheduler);
            scheduler.AdvanceBy(2);
            
            model.Open.CanExecute(null).Should().BeTrue();
            model.Close.CanExecute(null).Should().BeFalse();
            model.IsVisible.Should().BeFalse();
            
            model.Open.Execute(null);
            scheduler.AdvanceBy(2);
            model.Open.CanExecute(null).Should().BeFalse();
            model.Close.CanExecute(null).Should().BeTrue();
            model.IsVisible.Should().BeTrue();

            model.Close.Execute(null);
            scheduler.AdvanceBy(2);
            model.Open.CanExecute(null).Should().BeTrue();
            model.Close.CanExecute(null).Should().BeFalse();
            model.IsVisible.Should().BeFalse();
        });

        [Fact]
        public void ShouldRenameFileSuccessfullyAndCloseViewModel() => new TestScheduler().With(scheduler =>
        {
            _providerViewModel.CanInteract.Returns(true);
            _providerViewModel.SelectedFile.Returns(_file);
            _file.Name.Returns("foo");
            
            var model = BuildRenameFileViewModel(scheduler);
            scheduler.AdvanceBy(2);

            model.OldName.Should().Be("foo");
            model.IsVisible.Should().BeFalse();
            model.Close.CanExecute(null).Should().BeFalse();
            model.Open.CanExecute(null).Should().BeTrue();
            model.Open.Execute(null);
            scheduler.AdvanceBy(3);

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
            scheduler.AdvanceBy(2);
            
            model.IsLoading.Should().BeTrue();
            model.Rename.CanExecute(null).Should().BeFalse();
            scheduler.AdvanceBy(3);
            
            model.IsLoading.Should().BeFalse();
            model.Rename.CanExecute(null).Should().BeFalse();
            model.NewName.Should().BeNullOrEmpty();
            model.OldName.Should().Be("foo");
            model.IsVisible.Should().BeFalse();
            
            model.Close.CanExecute(null).Should().BeFalse();
            model.Open.CanExecute(null).Should().BeTrue();
        });

        private RenameFileViewModel BuildRenameFileViewModel(IScheduler scheduler)
        {
            return new RenameFileViewModel(_providerViewModel, _provider, scheduler, scheduler);
        }
    }
}