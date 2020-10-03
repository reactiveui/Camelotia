using System;
using System.Linq;
using System.Reactive.Concurrency;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using DynamicData;
using Akavache;
using FluentAssertions;
using NSubstitute;
using ReactiveUI;
using Xunit;

namespace Camelotia.Tests.Presentation
{
    public sealed class MainViewModelTests
    {
        private readonly MainState _state = new MainState();
        
        [Fact]
        public void ShouldIndicateWhenLoadingAndReady() 
        {
            var model = BuildMainViewModel();
            model.IsLoading.Should().BeFalse();
            model.IsReady.Should().BeTrue();
                
            model.Refresh.CanExecute().Should().BeTrue();
            model.Refresh.Execute().Subscribe();
                
            model.Providers.Should().BeEmpty();
            model.IsLoading.Should().BeFalse();
            model.IsReady.Should().BeTrue();
        }

        [Fact]
        public void ShouldSelectProviderFromStateWhenProvidersGetLoaded()
        {
            var provider = new ProviderState();
            _state.Providers.AddOrUpdate(provider);
            _state.SelectedProviderId = provider.Id;
                
            var model = BuildMainViewModel();
            model.Providers.Should().NotBeEmpty();
            model.SelectedProvider.Should().NotBeNull();
            model.SelectedProvider.Id.Should().Be(provider.Id);
            model.Refresh.Execute().Subscribe();
                
            model.Providers.Should().NotBeEmpty();
            model.SelectedProvider.Should().NotBeNull();
            model.SelectedProvider.Id.Should().Be(provider.Id);
        }

        [Fact]
        public void ShouldUnselectSelectedProvider() 
        {
            var provider = new ProviderState();
            _state.Providers.AddOrUpdate(provider);
            _state.SelectedProviderId = provider.Id;

            var model = BuildMainViewModel();
            model.Providers.Should().NotBeEmpty();
            model.SelectedProvider.Should().NotBeNull();
            model.SelectedProvider.Id.Should().Be(provider.Id);
            model.Unselect.CanExecute().Should().BeTrue();
            model.Unselect.Execute().Subscribe();

            model.Providers.Should().NotBeEmpty();
            model.SelectedProvider.Should().BeNull();
        }

        [Fact]
        public void ShouldOrderProvidersBasedOnDateAdded()
        {
            _state.Providers.AddOrUpdate(new[]
            {
                new ProviderState { Created = new DateTime(2000, 1, 1, 1, 1, 1) },
                new ProviderState { Created = new DateTime(2015, 1, 1, 1, 1, 1) },
                new ProviderState { Created = new DateTime(2010, 1, 1, 1, 1, 1) }
            });

            var model = BuildMainViewModel();
            model.Providers.Should().NotBeEmpty();
            model.Providers.Count.Should().Be(3);

            model.Providers[0].Created.Should().Be(new DateTime(2015, 1, 1, 1, 1, 1));
            model.Providers[1].Created.Should().Be(new DateTime(2010, 1, 1, 1, 1, 1));
            model.Providers[2].Created.Should().Be(new DateTime(2000, 1, 1, 1, 1, 1));
        }

        [Fact]
        public void ShouldUnselectProviderOnceDeleted()
        {
            var provider = new ProviderState();
            _state.Providers.AddOrUpdate(new ProviderState());
            _state.Providers.AddOrUpdate(provider);
            _state.SelectedProviderId = provider.Id;
            
            var model = BuildMainViewModel();
            model.Providers.Count.Should().Be(2);
            model.SelectedProvider.Should().NotBeNull();
            model.SelectedProvider.Id.Should().Be(provider.Id);
            model.Remove.Execute().Subscribe();

            model.SelectedProvider.Should().BeNull();
            model.Providers.Count.Should().Be(1);
        }

        [Fact]
        public void ShouldAddNewProviders()
        {
            var model = BuildMainViewModel();
            model.Providers.Should().BeEmpty();
            model.SelectedProvider.Should().BeNull();
            model.SelectedSupportedType = ProviderType.Local;
            model.Add.Execute().Subscribe();

            model.Providers.Should().NotBeEmpty();
            model.Providers.Count.Should().Be(1);
        }
        
        [Fact]
        public void ShouldSynchronizeSelectedTypeWithState()
        {
            _state.SelectedSupportedType = ProviderType.Local;
            
            var model = BuildMainViewModel();
            model.SelectedSupportedType.Should().Be(ProviderType.Local);
            model.SelectedSupportedType = ProviderType.Ftp;
            _state.SelectedSupportedType.Should().Be(ProviderType.Ftp);

            model.SelectedSupportedType = ProviderType.Local;
            _state.SelectedSupportedType.Should().Be(ProviderType.Local);
        }

        [Fact]
        public void ShouldSaveUserSelectionToStateObject()
        {
            _state.Providers.AddOrUpdate(new ProviderState());
            _state.Providers.AddOrUpdate(new ProviderState());
            
            var model = BuildMainViewModel();
            model.SelectedProvider.Should().BeNull();
            _state.SelectedProviderId.Should().BeEmpty();

            model.SelectedProvider = model.Providers.First();
            _state.SelectedProviderId.Should().NotBeEmpty();
            _state.SelectedProviderId.Should().Be(model.SelectedProvider.Id);
        }

        private MainViewModel BuildMainViewModel()
        {
            RxApp.MainThreadScheduler = Scheduler.Immediate;
            RxApp.TaskpoolScheduler = Scheduler.Immediate;
            return new MainViewModel(_state, new ProviderFactory(
                Substitute.For<IAuthenticator>(), 
                Substitute.For<IBlobCache>()), 
                (state, provider) =>
            {
                var entry = Substitute.For<IProviderViewModel>();
                entry.Created.Returns(provider.Created);
                entry.Id.Returns(provider.Id);
                return entry;
            });
        }
    }
}