using System.IO;
using System.Reactive.Concurrency;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using ReactiveUI.Testing;
using NSubstitute;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class CreateFolderViewModelTests
    {
        private static readonly string Separator = Path.DirectorySeparatorChar.ToString();
        private readonly IProviderViewModel _providerViewModel = Substitute.For<IProviderViewModel>();
        private readonly IProvider _provider = Substitute.For<IProvider>();

        [Fact]
        public void ShouldProperlyInitializeCreateFolderViewModel() => new TestScheduler().With(scheduler =>
        {
            var model = BuildCreateFolderViewModel(scheduler);
            model.Name.Should().BeNullOrEmpty();
            model.Path.Should().BeNullOrEmpty();
            
            model.ErrorMessage.Should().BeNullOrEmpty();
            model.HasErrors.Should().BeFalse();
            model.IsVisible.Should().BeFalse();
        });

        [Fact]
        public void ShouldChangeVisibility() => new TestScheduler().With(scheduler =>
        {
            _providerViewModel.CanInteract.Returns(true);
            _providerViewModel.CurrentPath.Returns(Separator);
            _provider.CanCreateFolder.Returns(true);
            
            var model = BuildCreateFolderViewModel(scheduler);
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
        public void ShouldCreateFolderSuccessfullyAndCloseViewModel() => new TestScheduler().With(scheduler =>
        {
            _providerViewModel.CanInteract.Returns(true);
            _providerViewModel.CurrentPath.Returns(Separator);
            _provider.CanCreateFolder.Returns(true);

            var model = BuildCreateFolderViewModel(scheduler);
            scheduler.AdvanceBy(2);
            
            model.IsVisible.Should().BeFalse();
            model.Close.CanExecute(null).Should().BeFalse();
            model.Open.CanExecute(null).Should().BeTrue();
            model.Open.Execute(null);
            scheduler.AdvanceBy(3);

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
            scheduler.AdvanceBy(2);
            
            model.IsLoading.Should().BeTrue();
            model.Create.CanExecute(null).Should().BeFalse();
            scheduler.AdvanceBy(3);
            
            model.IsLoading.Should().BeFalse();
            model.Create.CanExecute(null).Should().BeFalse();
            model.Name.Should().BeNullOrEmpty();
            model.Path.Should().Be(Separator);
            model.IsVisible.Should().BeFalse();
            
            model.Close.CanExecute(null).Should().BeFalse();
            model.Open.CanExecute(null).Should().BeTrue();
        });

        private CreateFolderViewModel BuildCreateFolderViewModel(IScheduler scheduler)
        {
            return new CreateFolderViewModel(_providerViewModel, scheduler, scheduler, _provider);
        }
    }
}