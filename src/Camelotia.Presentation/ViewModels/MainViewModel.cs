using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
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
        private readonly ReactiveCommand<Unit, ProviderState> _add;
        private readonly ReactiveCommand<Unit, Unit> _unselect;
        private readonly ReactiveCommand<Unit, Unit> _refresh;
        private readonly ReactiveCommand<Unit, Guid> _remove;
        private readonly IProviderFactory _factory;

        public MainViewModel(MainState state, IProviderFactory factory, ProviderViewModelFactory createViewModel)
        {
            _factory = factory;
            _refresh = ReactiveCommand.Create(state.Providers.Refresh);
            _refresh.IsExecuting.ToPropertyEx(this, x => x.IsLoading);
            
            _refresh.IsExecuting
                .Select(executing => !executing)
                .ToPropertyEx(this, x => x.IsReady);
            
            state.Providers.Connect()
                .Transform(ps => createViewModel(ps, factory.CreateProvider(ps.Parameters)))
                .Sort(SortExpressionComparer<IProviderViewModel>.Descending(x => x.Created))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _providers)
                .Subscribe();

            var canRemove = this
                .WhenAnyValue(x => x.SelectedProvider)
                .Select(provider => provider != null);
            
            _remove = ReactiveCommand.Create(() => SelectedProvider.Id, canRemove);
            _remove.Subscribe(state.Providers.RemoveKey);

            var canAddProvider = this
                .WhenAnyValue(x => x.SelectedSupportedType)
                .Select(type => Enum.IsDefined(typeof(ProviderType), type));
            
            _add = ReactiveCommand.Create(
                () => new ProviderState { Type = SelectedSupportedType },
                canAddProvider);

            _add.Subscribe(state.Providers.AddOrUpdate);

            this.WhenAnyValue(x => x.SelectedProvider)
                .Select(provider => provider == null)
                .ToPropertyEx(this, x => x.WelcomeScreenVisible);

            this.WhenAnyValue(x => x.WelcomeScreenVisible)
                .Select(visible => !visible)
                .ToPropertyEx(this, x => x.WelcomeScreenCollapsed);

            var canUnselect = this
                .WhenAnyValue(x => x.SelectedProvider)
                .Select(provider => provider != null);

            _unselect = ReactiveCommand.Create(() => Unit.Default, canUnselect);
            _unselect.Subscribe(unit => SelectedProvider = null);
            
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

        [ObservableAsProperty]
        public bool WelcomeScreenCollapsed { get; }

        [ObservableAsProperty]
        public bool WelcomeScreenVisible { get; }

        [ObservableAsProperty]
        public bool IsLoading { get; }
        
        [ObservableAsProperty]
        public bool IsReady { get; }

        public ReadOnlyObservableCollection<IProviderViewModel> Providers => _providers;

        public IEnumerable<ProviderType> SupportedTypes => _factory.SupportedTypes;

        public ICommand Unselect => _unselect;

        public ICommand Refresh => _refresh;

        public ICommand Remove => _remove;

        public ICommand Add => _add;
    }
}