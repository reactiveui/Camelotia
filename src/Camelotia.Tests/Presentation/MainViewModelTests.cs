using System;
using System.Linq;
using System.Reactive.Concurrency;
using Akavache;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services;
using Camelotia.Services.Interfaces;
using DynamicData;
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
            model.IsReady.Should().BeFalse();
                
            model.Refresh.CanExecute(null).Should().BeTrue();
            model.Refresh.Execute(null);
                
            model.Providers.Should().BeEmpty();
            model.IsLoading.Should().BeFalse();
            model.IsReady.Should().BeTrue();
        }

        [Fact]
        public void ShouldSelectFirstProviderWhenProvidersGetLoaded()
        {
            _state.Providers.AddOrUpdate(new ProviderState());
                
            var model = BuildMainViewModel();
            model.Providers.Should().NotBeEmpty();
            model.SelectedProvider.Should().NotBeNull();
            model.Refresh.Execute(null);
                
            model.Providers.Should().NotBeEmpty();
            model.SelectedProvider.Should().NotBeNull();
        }

        [Fact]
        public void ShouldUnselectSelectedProvider() 
        {
            _state.Providers.AddOrUpdate(new ProviderState());

            var model = BuildMainViewModel();
            model.Providers.Should().NotBeEmpty();
            model.SelectedProvider.Should().NotBeNull();
            model.Unselect.CanExecute(null).Should().BeTrue();
            model.Unselect.Execute(null);

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
            _state.Providers.AddOrUpdate(new ProviderState());
            _state.Providers.AddOrUpdate(new ProviderState());
            
            var model = BuildMainViewModel();
            model.Providers.Count.Should().Be(2);
            model.SelectedProvider.Should().NotBeNull();
            model.Remove.Execute(null);

            model.SelectedProvider.Should().BeNull();
            model.Providers.Count.Should().Be(1);
            model.SelectedProvider = model.Providers.First();
            model.Remove.Execute(null);

            model.SelectedProvider.Should().BeNull();
            model.Providers.Should().BeEmpty();
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