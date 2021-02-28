using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Camelotia.Presentation.ViewModels
{
    public sealed class MainViewModel : ReactiveObject, IMainViewModel
    {
        private readonly ReadOnlyObservableCollection<ICloudViewModel> _providers;
        private readonly ObservableAsPropertyHelper<bool> _welcomeScreenCollapsed;
        private readonly ObservableAsPropertyHelper<bool> _welcomeScreenVisible;
        private readonly ObservableAsPropertyHelper<bool> _isLoading;
        private readonly ObservableAsPropertyHelper<bool> _isReady;
        private readonly ICloudFactory _factory;

        public MainViewModel(MainState state, ICloudFactory factory, CloudViewModelFactory createViewModel)
        {
            _factory = factory;
            Refresh = ReactiveCommand.Create(state.Clouds.Refresh);

            _isLoading = Refresh
                .IsExecuting
                .ToProperty(this, x => x.IsLoading);

            _isReady = Refresh
                .IsExecuting
                .Select(executing => !executing)
                .ToProperty(this, x => x.IsReady);

            state
                .Clouds
                .Connect()
                .Transform(ps => createViewModel(ps, factory.CreateCloud(ps.Parameters)))
                .Sort(SortExpressionComparer<ICloudViewModel>.Descending(x => x.Created))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _providers)
                .Subscribe();

            var canRemove = this
                .WhenAnyValue(x => x.SelectedProvider)
                .Select(provider => provider != null);

            Remove = ReactiveCommand.Create(
                () => state.Clouds.RemoveKey(SelectedProvider.Id),
                canRemove);

            var canAddProvider = this
                .WhenAnyValue(x => x.SelectedSupportedType)
                .Select(type => Enum.IsDefined(typeof(CloudType), type));

            Add = ReactiveCommand.Create(
                () => state.Clouds.AddOrUpdate(new CloudState { Type = SelectedSupportedType }),
                canAddProvider);

            _welcomeScreenVisible = this
                .WhenAnyValue(x => x.SelectedProvider)
                .Select(provider => provider == null)
                .ToProperty(this, x => x.WelcomeScreenVisible);

            _welcomeScreenCollapsed = this
                .WhenAnyValue(x => x.WelcomeScreenVisible)
                .Select(visible => !visible)
                .ToProperty(this, x => x.WelcomeScreenCollapsed);

            var canUnselect = this
                .WhenAnyValue(x => x.SelectedProvider)
                .Select(provider => provider != null);

            Unselect = ReactiveCommand.Create(() => Unit.Default, canUnselect);
            Unselect.Subscribe(unit => SelectedProvider = null);

            var outputCollectionChanges = Clouds
                .ToObservableChangeSet(x => x.Id)
                .Publish()
                .RefCount();

            outputCollectionChanges
                .Filter(provider => provider.Id == state.SelectedProviderId)
                .ObserveOn(RxApp.MainThreadScheduler)
                .OnItemAdded(provider => SelectedProvider = provider)
                .Subscribe();

            outputCollectionChanges
                .OnItemRemoved(provider => SelectedProvider = null)
                .Subscribe();

            this.WhenAnyValue(x => x.SelectedProvider)
                .Skip(1)
                .Select(provider => provider?.Id ?? Guid.Empty)
                .Subscribe(id => state.SelectedProviderId = id);

            SelectedSupportedType = state.SelectedSupportedType ?? SupportedTypes.First();
            this.WhenAnyValue(x => x.SelectedSupportedType)
                .Subscribe(type => state.SelectedSupportedType = type);
        }

        [Reactive]
        public CloudType SelectedSupportedType { get; set; }

        [Reactive]
        public ICloudViewModel SelectedProvider { get; set; }

        public ReactiveCommand<Unit, Unit> Unselect { get; }

        public ReactiveCommand<Unit, Unit> Refresh { get; }

        public ReactiveCommand<Unit, Unit> Remove { get; }

        public ReactiveCommand<Unit, Unit> Add { get; }

        public ReadOnlyObservableCollection<ICloudViewModel> Clouds => _providers;

        public IEnumerable<CloudType> SupportedTypes => _factory.SupportedClouds;

        public bool WelcomeScreenCollapsed => _welcomeScreenCollapsed.Value;

        public bool WelcomeScreenVisible => _welcomeScreenVisible.Value;

        public bool IsLoading => _isLoading.Value;

        public bool IsReady => _isReady.Value;
    }
}
