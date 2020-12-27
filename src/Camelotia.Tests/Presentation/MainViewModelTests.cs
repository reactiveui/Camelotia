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
                
            model.Clouds.Should().BeEmpty();
            model.IsLoading.Should().BeFalse();
            model.IsReady.Should().BeTrue();
        }

        [Fact]
        public void ShouldSelectProviderFromStateWhenProvidersGetLoaded()
        {
            var provider = new CloudState();
            _state.Clouds.AddOrUpdate(provider);
            _state.SelectedProviderId = provider.Id;
                
            var model = BuildMainViewModel();
            model.Clouds.Should().NotBeEmpty();
            model.SelectedProvider.Should().NotBeNull();
            model.SelectedProvider.Id.Should().Be(provider.Id);
            model.Refresh.Execute().Subscribe();
                
            model.Clouds.Should().NotBeEmpty();
            model.SelectedProvider.Should().NotBeNull();
            model.SelectedProvider.Id.Should().Be(provider.Id);
        }

        [Fact]
        public void ShouldUnselectSelectedProvider() 
        {
            var provider = new CloudState();
            _state.Clouds.AddOrUpdate(provider);
            _state.SelectedProviderId = provider.Id;

            var model = BuildMainViewModel();
            model.Clouds.Should().NotBeEmpty();
            model.SelectedProvider.Should().NotBeNull();
            model.SelectedProvider.Id.Should().Be(provider.Id);
            model.Unselect.CanExecute().Should().BeTrue();
            model.Unselect.Execute().Subscribe();

            model.Clouds.Should().NotBeEmpty();
            model.SelectedProvider.Should().BeNull();
        }

        [Fact]
        public void ShouldOrderProvidersBasedOnDateAdded()
        {
            _state.Clouds.AddOrUpdate(new[]
            {
                new CloudState { Created = new DateTime(2000, 1, 1, 1, 1, 1) },
                new CloudState { Created = new DateTime(2015, 1, 1, 1, 1, 1) },
                new CloudState { Created = new DateTime(2010, 1, 1, 1, 1, 1) }
            });

            var model = BuildMainViewModel();
            model.Clouds.Should().NotBeEmpty();
            model.Clouds.Count.Should().Be(3);

            model.Clouds[0].Created.Should().Be(new DateTime(2015, 1, 1, 1, 1, 1));
            model.Clouds[1].Created.Should().Be(new DateTime(2010, 1, 1, 1, 1, 1));
            model.Clouds[2].Created.Should().Be(new DateTime(2000, 1, 1, 1, 1, 1));
        }

        [Fact]
        public void ShouldUnselectProviderOnceDeleted()
        {
            var provider = new CloudState();
            _state.Clouds.AddOrUpdate(new CloudState());
            _state.Clouds.AddOrUpdate(provider);
            _state.SelectedProviderId = provider.Id;
            
            var model = BuildMainViewModel();
            model.Clouds.Count.Should().Be(2);
            model.SelectedProvider.Should().NotBeNull();
            model.SelectedProvider.Id.Should().Be(provider.Id);
            model.Remove.Execute().Subscribe();

            model.SelectedProvider.Should().BeNull();
            model.Clouds.Count.Should().Be(1);
        }

        [Fact]
        public void ShouldAddNewProviders()
        {
            var model = BuildMainViewModel();
            model.Clouds.Should().BeEmpty();
            model.SelectedProvider.Should().BeNull();
            model.SelectedSupportedType = CloudType.Local;
            model.Add.Execute().Subscribe();

            model.Clouds.Should().NotBeEmpty();
            model.Clouds.Count.Should().Be(1);
        }
        
        [Fact]
        public void ShouldSynchronizeSelectedTypeWithState()
        {
            _state.SelectedSupportedType = CloudType.Local;
            
            var model = BuildMainViewModel();
            model.SelectedSupportedType.Should().Be(CloudType.Local);
            model.SelectedSupportedType = CloudType.Ftp;
            _state.SelectedSupportedType.Should().Be(CloudType.Ftp);

            model.SelectedSupportedType = CloudType.Local;
            _state.SelectedSupportedType.Should().Be(CloudType.Local);
        }

        [Fact]
        public void ShouldSaveUserSelectionToStateObject()
        {
            _state.Clouds.AddOrUpdate(new CloudState());
            _state.Clouds.AddOrUpdate(new CloudState());
            
            var model = BuildMainViewModel();
            model.SelectedProvider.Should().BeNull();
            _state.SelectedProviderId.Should().BeEmpty();

            model.SelectedProvider = model.Clouds.First();
            _state.SelectedProviderId.Should().NotBeEmpty();
            _state.SelectedProviderId.Should().Be(model.SelectedProvider.Id);
        }

        private MainViewModel BuildMainViewModel()
        {
            RxApp.MainThreadScheduler = Scheduler.Immediate;
            RxApp.TaskpoolScheduler = Scheduler.Immediate;
            return new MainViewModel(_state, new CloudFactory(
                Substitute.For<IAuthenticator>(), 
                Substitute.For<IBlobCache>()), 
                (state, provider) =>
            {
                var entry = Substitute.For<ICloudViewModel>();
                entry.Created.Returns(provider.Created);
                entry.Id.Returns(provider.Id);
                return entry;
            });
        }
    }
}