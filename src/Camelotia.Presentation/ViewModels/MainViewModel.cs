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
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using DynamicData;
using DynamicData.Binding;

namespace Camelotia.Presentation.ViewModels
{
    public sealed class MainViewModel : ReactiveObject, IMainViewModel
    {
        private readonly ReadOnlyObservableCollection<IProviderViewModel> _providers;
        private readonly ObservableAsPropertyHelper<bool> _welcomeScreenCollapsed;
        private readonly ObservableAsPropertyHelper<bool> _welcomeScreenVisible;
        private readonly ObservableAsPropertyHelper<bool> _isLoading;
        private readonly ObservableAsPropertyHelper<bool> _isReady;
        private readonly IProviderFactory _factory;

        public MainViewModel(MainState state, IProviderFactory factory, ProviderViewModelFactory createViewModel)
        {
            _factory = factory;
            Refresh = ReactiveCommand.Create(state.Providers.Refresh);
            
            _isLoading = Refresh
                .IsExecuting
                .ToProperty(this, x => x.IsLoading);
            
            _isReady = Refresh
                .IsExecuting
                .Select(executing => !executing)
                .ToProperty(this, x => x.IsReady);
            
            state.Providers.Connect()
                .Transform(ps => createViewModel(ps, factory.CreateProvider(ps.Parameters)))
                .Sort(SortExpressionComparer<IProviderViewModel>.Descending(x => x.Created))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _providers)
                .Subscribe();

            var canRemove = this
                .WhenAnyValue(x => x.SelectedProvider)
                .Select(provider => provider != null);
            
            Remove = ReactiveCommand.Create(
                () => state.Providers.RemoveKey(SelectedProvider.Id), 
                canRemove);

            var canAddProvider = this
                .WhenAnyValue(x => x.SelectedSupportedType)
                .Select(type => Enum.IsDefined(typeof(ProviderType), type));
            
            Add = ReactiveCommand.Create(
                () => state.Providers.AddOrUpdate(new ProviderState { Type = SelectedSupportedType }),
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
            
            var outputCollectionChanges = Providers
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
        public ProviderType SelectedSupportedType { get; set; }

        [Reactive] 
        public IProviderViewModel SelectedProvider { get; set; }

        public ReactiveCommand<Unit, Unit> Unselect { get; }

        public ReactiveCommand<Unit, Unit> Refresh { get; }

        public ReactiveCommand<Unit, Unit> Remove { get; }

        public ReactiveCommand<Unit, Unit> Add { get; }

        public ReadOnlyObservableCollection<IProviderViewModel> Providers => _providers;

        public IEnumerable<ProviderType> SupportedTypes => _factory.SupportedTypes;

        public bool WelcomeScreenCollapsed => _welcomeScreenCollapsed.Value;

        public bool WelcomeScreenVisible => _welcomeScreenVisible.Value;
        
        public bool IsLoading => _isLoading.Value;

        public bool IsReady => _isReady.Value;
    }
}