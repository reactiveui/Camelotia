using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using DynamicData;
using DynamicData.Binding;
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
            _providerStorage.Read().Returns(Observable.Return(new ChangeSet<IProvider, Guid>()));
            
            var model = BuildMainViewModel(scheduler);
            model.IsLoading.Should().BeFalse();
            model.IsReady.Should().BeFalse();
                
            model.Refresh.CanExecute(null).Should().BeTrue();
            model.Refresh.Execute(null);
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
            var collection = new ObservableCollectionExtended<IProvider>();
            var set = collection.ToObservableChangeSet(x => x.Id);
            
            _providerStorage.Read().Returns(set);
            _providerStorage
                .When(storage => storage.Refresh())
                .Do(args => collection.Add(Substitute.For<IProvider>()));
                
            var model = BuildMainViewModel(scheduler);
            scheduler.AdvanceBy(2);

            model.Providers.Should().BeEmpty();
            model.Refresh.Execute(null);
            scheduler.AdvanceBy(3);
                
            model.Providers.Should().NotBeEmpty();
            model.SelectedProvider.Should().NotBeNull();
        });

        [Fact]
        public void ActivationShouldTriggerLoad() => new TestScheduler().With(scheduler =>
        {
            var collection = new ObservableCollectionExtended<IProvider>();
            var set = collection.ToObservableChangeSet(x => x.Id);
            
            _providerStorage.Read().Returns(set);
            _providerStorage
                .When(storage => storage.Refresh())
                .Do(args => collection.Add(Substitute.For<IProvider>()));
                
            var model = BuildMainViewModel(scheduler);
            scheduler.AdvanceBy(2);
                
            model.Providers.Should().BeEmpty();
            model.Activator.Activate();
            scheduler.AdvanceBy(4);
                
            model.Providers.Should().NotBeEmpty();
            model.SelectedProvider.Should().NotBeNull();
        });

        [Fact]
        public void ShouldUnselectSelectedProvider() => new TestScheduler().With(scheduler =>
        {
            var collection = new ObservableCollectionExtended<IProvider>();
            var changes = collection.ToObservableChangeSet(x => x.Id);

            _providerStorage.Read().Returns(changes);
            _providerStorage
                .When(storage => storage.Refresh())
                .Do(args => collection.Add(Substitute.For<IProvider>()));

            var model = BuildMainViewModel(scheduler);
            scheduler.AdvanceBy(2);

            model.Providers.Should().BeEmpty();
            model.Refresh.Execute(null);
            scheduler.AdvanceBy(3);

            model.Providers.Should().NotBeEmpty();
            model.SelectedProvider.Should().NotBeNull();
            model.Unselect.CanExecute(null).Should().BeTrue();
            model.Unselect.Execute(null);

            scheduler.AdvanceBy(3);
            model.Providers.Should().NotBeEmpty();
            model.SelectedProvider.Should().BeNull();
        });

        [Fact]
        public void ShouldOrderProvidersBasedOnDateAdded() => new TestScheduler().With(scheduler =>
        {
            var collection = new ObservableCollectionExtended<IProvider>
            {
                BuildProviderCreatedAt(new DateTime(2000, 1, 1, 1, 1, 1)),
                BuildProviderCreatedAt(new DateTime(2015, 1, 1, 1, 1, 1)),
                BuildProviderCreatedAt(new DateTime(2010, 1, 1, 1, 1, 1))
            };
            var changes = collection.ToObservableChangeSet(x => x.Id);
            _providerStorage.Read().Returns(changes);

            var model = BuildMainViewModel(scheduler, (provider, _, __) =>
            {
                var id = provider.Id;
                var created = provider.Created;
                var entry = Substitute.For<IProviderViewModel>();
                entry.Created.Returns(created);
                entry.Id.Returns(id);
                return entry;
            });

            model.Providers.Should().BeEmpty();
            model.Refresh.Execute(null);
            scheduler.AdvanceBy(3);

            model.Providers.Should().NotBeEmpty();
            model.Providers.Count.Should().Be(3);

            model.Providers[0].Created.Should().Be(new DateTime(2015, 1, 1, 1, 1, 1));
            model.Providers[1].Created.Should().Be(new DateTime(2010, 1, 1, 1, 1, 1));
            model.Providers[2].Created.Should().Be(new DateTime(2000, 1, 1, 1, 1, 1));
        });

        private IProvider BuildProviderCreatedAt(DateTime date)
        {
            var id = Guid.NewGuid();
            var provider = Substitute.For<IProvider>();
            provider.Created.Returns(date);
            provider.Id.Returns(id);
            return provider;
        }

        private MainViewModel BuildMainViewModel(IScheduler scheduler, ProviderViewModelFactory factory = null)
        {
            return new MainViewModel(
                factory ?? ((provider, files, auth) => Substitute.For<IProviderViewModel>()),
                provider => Substitute.For<IAuthViewModel>(),
                _providerStorage,
                _fileManager,
                scheduler,
                scheduler
            );
        }
    }
}