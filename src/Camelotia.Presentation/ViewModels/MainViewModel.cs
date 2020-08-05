using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using DynamicData;
using DynamicData.Binding;

namespace Camelotia.Presentation.ViewModels
{
    public sealed class MainViewModel : ReactiveObject, IMainViewModel, IActivatableViewModel
    {
        private readonly ReadOnlyObservableCollection<IProviderViewModel> _providers;
        private readonly ReactiveCommand<Unit, Unit> _unselect;
        private readonly ReactiveCommand<Unit, Unit> _refresh;
        private readonly ReactiveCommand<Unit, Unit> _remove;
        private readonly ReactiveCommand<Unit, Unit> _add;
        private readonly IStorage _storage;

        public MainViewModel(
            ProviderViewModelFactory providerFactory,
            AuthViewModelFactory authFactory,
            IStorage storage)
        {
            _storage = storage;
            _refresh = ReactiveCommand.CreateFromTask(storage.Refresh);
            
            var providers = storage.Read();
            providers.Transform(x => providerFactory(x, authFactory(x)))
                .Sort(SortExpressionComparer<IProviderViewModel>.Descending(x => x.Created))
                .StartWithEmpty()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _providers)
                .Subscribe();
            
            _refresh.IsExecuting.ToPropertyEx(this, x => x.IsLoading);
            
            _refresh.IsExecuting
                .Skip(1)
                .Select(executing => !executing)
                .ToPropertyEx(this, x => x.IsReady);

            providers.Where(changes => changes.Any())
                .ObserveOn(RxApp.MainThreadScheduler)
                .OnItemAdded(x => SelectedProvider = Providers.FirstOrDefault())
                .OnItemRemoved(x => SelectedProvider = null)
                .Subscribe();

            var canRemove = this
                .WhenAnyValue(x => x.SelectedProvider)
                .Select(provider => provider != null);
            
            _remove = ReactiveCommand.CreateFromTask(
                () => storage.Remove(SelectedProvider.Id),
                canRemove);

            var canAddProvider = this
                .WhenAnyValue(x => x.SelectedSupportedType)
                .Select(type => !string.IsNullOrWhiteSpace(type));
            
            _add = ReactiveCommand.CreateFromTask(
                () => storage.Add(SelectedSupportedType),
                canAddProvider);

            this.WhenAnyValue(x => x.SelectedProvider)
                .Select(provider => provider == null)
                .ToPropertyEx(this, x => x.WelcomeScreenVisible);

            this.WhenAnyValue(x => x.WelcomeScreenVisible)
                .Select(visible => !visible)
                .ToPropertyEx(this, x => x.WelcomeScreenCollapsed);

            var canUnSelect = this
                .WhenAnyValue(x => x.SelectedProvider)
                .Select(provider => provider != null);

            _unselect = ReactiveCommand.Create(
                () => { SelectedProvider = null; }, 
                canUnSelect);
            
            Activator = new ViewModelActivator();
            this.WhenActivated(disposables =>
            {
                SelectedSupportedType = SupportedTypes.FirstOrDefault();
                _refresh.Execute().Subscribe(o => { }, e => { }).DisposeWith(disposables);
            });
        }
        
        [Reactive] 
        public string SelectedSupportedType { get; set; }

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

        public ViewModelActivator Activator { get; }
        
        public ReadOnlyObservableCollection<IProviderViewModel> Providers => _providers;

        public IEnumerable<string> SupportedTypes => _storage.SupportedTypes;

        public ICommand Unselect => _unselect;

        public ICommand Refresh => _refresh;

        public ICommand Remove => _remove;

        public ICommand Add => _add;
    }
}