using System.Linq;
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
    public sealed class MainViewModelTests
    {
        private readonly IProviderStorage _providerStorage = Substitute.For<IProviderStorage>();
        private readonly IFileManager _fileManager = Substitute.For<IFileManager>();

        [Fact]
        public void ShouldIndicateWhenLoadingAndReady() => new TestScheduler().With(scheduler =>
        {
            _providerStorage
                .LoadProviders()
                .Returns(Enumerable.Empty<IProvider>());
                
            var model = BuildMainViewModel(scheduler);
            model.IsLoading.Should().BeFalse();
            model.IsReady.Should().BeFalse();
                
            model.LoadProviders.CanExecute(null).Should().BeTrue();
            model.LoadProviders.Execute(null);
            scheduler.AdvanceBy(2);
                
            model.Providers.Should().BeEmpty();
            model.IsLoading.Should().BeTrue();
            model.IsReady.Should().BeFalse();
            scheduler.AdvanceBy(2);
                
            model.Providers.Should().BeEmpty();
            model.IsLoading.Should().BeFalse();
            model.IsReady.Should().BeTrue();
        });

        [Fact]
        public void ShouldSelectFirstProviderWhenProvidersGetLoaded() => new TestScheduler().With(scheduler =>
        {
            var providers = Enumerable.Repeat(Substitute.For<IProvider>(), 1);
            _providerStorage
                .LoadProviders()
                .Returns(providers);
                
            var model = BuildMainViewModel(scheduler);
            scheduler.AdvanceBy(2);

            model.Providers.Should().BeEmpty();
            model.LoadProviders.Execute(null);
            scheduler.AdvanceBy(3);
                
            model.Providers.Should().NotBeEmpty();
            model.SelectedProvider.Should().NotBeNull();
        });

        [Fact]
        public void ActivationShouldTriggerLoad() => new TestScheduler().With(scheduler =>
        {
            var providers = Enumerable.Repeat(Substitute.For<IProvider>(), 1);
            _providerStorage
                .LoadProviders()
                .Returns(providers);
                
            var model = BuildMainViewModel(scheduler);
            scheduler.AdvanceBy(2);
                
            model.Providers.Should().BeEmpty();
            model.Activator.Activate();
            scheduler.AdvanceBy(4);
                
            model.Providers.Should().NotBeEmpty();
            model.SelectedProvider.Should().NotBeNull();
        });

        private MainViewModel BuildMainViewModel(IScheduler scheduler)
        {
            return new MainViewModel(
                (provider, files, auth) => Substitute.For<IProviderViewModel>(),
                provider => Substitute.For<IAuthViewModel>(),
                _providerStorage,
                _fileManager,
                scheduler,
                scheduler
            );
        }
    }
}