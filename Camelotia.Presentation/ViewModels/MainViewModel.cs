using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using DynamicData;

namespace Camelotia.Presentation.ViewModels
{
    public sealed class MainViewModel : ReactiveObject, IMainViewModel, ISupportsActivation
    {
        private readonly ReadOnlyObservableCollection<IProviderViewModel> _providers;
        private readonly ObservableAsPropertyHelper<bool> _welcomeScreenCollapsed;
        private readonly ObservableAsPropertyHelper<bool> _welcomeScreenVisible;
        private readonly ObservableAsPropertyHelper<bool> _isLoading;
        private readonly ObservableAsPropertyHelper<bool> _isReady;
        private readonly ReactiveCommand<Unit, Unit> _unselect;
        private readonly ReactiveCommand<Unit, Unit> _refresh;
        private readonly ReactiveCommand<Unit, Unit> _remove;
        private readonly IProviderStorage _providerStorage;
        private readonly ReactiveCommand<Unit, Unit> _add;

        public MainViewModel(
            Func<IProvider, IFileManager, IAuthViewModel, IProviderViewModel> providerFactory,
            Func<IProvider, IAuthViewModel> authFactory,
            IProviderStorage providerStorage, 
            IFileManager fileManager,
            IScheduler currentThread,
            IScheduler mainThread)
        {
            _providerStorage = providerStorage;
            _refresh = ReactiveCommand.CreateFromTask(
                providerStorage.Refresh,
                outputScheduler: mainThread);
            
            var providers = providerStorage.Providers();
            providers.Transform(x => providerFactory(x, fileManager, authFactory(x)))
                .ObserveOn(RxApp.MainThreadScheduler)
                .StartWithEmpty()
                .Bind(out _providers)
                .Subscribe();
            
            _isLoading = _refresh
                .IsExecuting
                .ToProperty(this, x => x.IsLoading, scheduler: currentThread);
            
            _isReady = _refresh
                .IsExecuting
                .Skip(1)
                .Select(executing => !executing)
                .ToProperty(this, x => x.IsReady, scheduler: currentThread);

            providers.Where(changes => changes.Any())
                .ObserveOn(RxApp.MainThreadScheduler)
                .OnItemAdded(x => SelectedProvider = Providers.LastOrDefault())
                .OnItemRemoved(x => SelectedProvider = null)
                .Subscribe();

            var canRemove = this
                .WhenAnyValue(x => x.SelectedProvider)
                .Select(provider => provider != null);
            
            _remove = ReactiveCommand.CreateFromTask(
                () => providerStorage.Remove(SelectedProvider.Id),
                canRemove);

            var canAddProvider = this
                .WhenAnyValue(x => x.SelectedSupportedType)
                .Select(type => !string.IsNullOrWhiteSpace(type));
            
            _add = ReactiveCommand.CreateFromTask(
                () => providerStorage.Add(SelectedSupportedType),
                canAddProvider);

            _welcomeScreenVisible = this
                .WhenAnyValue(x => x.SelectedProvider)
                .Select(provider => provider == null)
                .ToProperty(this, x => x.WelcomeScreenVisible);

            _welcomeScreenCollapsed = this
                .WhenAnyValue(x => x.WelcomeScreenVisible)
                .Select(visible => !visible)
                .ToProperty(this, x => x.WelcomeScreenCollapsed);

            var canUnSelect = this
                .WhenAnyValue(x => x.SelectedProvider)
                .Select(provider => provider != null);

            _unselect = ReactiveCommand.Create(
                () => { SelectedProvider = null; }, 
                canUnSelect);
            
            Activator = new ViewModelActivator();
            this.WhenActivated(async (CompositeDisposable disposable) =>
            {
                SelectedSupportedType = SupportedTypes.FirstOrDefault();
                await _refresh.Execute();
            });
        }

        public ViewModelActivator Activator { get; }
        
        [Reactive] public string SelectedSupportedType { get; set; }

        [Reactive] public IProviderViewModel SelectedProvider { get; set; }
        
        public ReadOnlyObservableCollection<IProviderViewModel> Providers => _providers;

        public IEnumerable<string> SupportedTypes => _providerStorage.SupportedTypes;

        public bool WelcomeScreenCollapsed => _welcomeScreenCollapsed.Value;

        public bool WelcomeScreenVisible => _welcomeScreenVisible.Value;

        public bool IsLoading => _isLoading.Value;
        
        public bool IsReady => _isReady.Value;

        public ICommand Unselect => _unselect;

        public ICommand Refresh => _refresh;

        public ICommand Remove => _remove;

        public ICommand Add => _add;
    }
}