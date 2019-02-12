using System;
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
        private readonly ObservableAsPropertyHelper<bool> _isLoading;
        private readonly ObservableAsPropertyHelper<bool> _isReady;
        private readonly ReactiveCommand<Unit, Unit> _load;

        public MainViewModel(
            Func<IProvider, IFileManager, IAuthViewModel, IProviderViewModel> providerFactory,
            Func<IProvider, IAuthViewModel> authFactory,
            IProviderStorage providerStorage, 
            IFileManager fileManager,
            IScheduler currentThread,
            IScheduler mainThread)
        {
            _load = ReactiveCommand.CreateFromTask(
                providerStorage.LoadProviders,
                outputScheduler: mainThread);
            
            var observableProviders = providerStorage.Connect();
            observableProviders
                .Transform(x => providerFactory(x, fileManager, authFactory(x)))
                .ObserveOn(RxApp.MainThreadScheduler)
                .StartWithEmpty()
                .Bind(out _providers)
                .Subscribe();
            
            _isLoading = _load
                .IsExecuting
                .ToProperty(this, x => x.IsLoading, scheduler: currentThread);
            
            _isReady = _load
                .IsExecuting
                .Skip(1)
                .Select(executing => !executing)
                .ToProperty(this, x => x.IsReady, scheduler: currentThread);

            observableProviders
                .Take(1)
                .Where(changes => changes.Any())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(changes => Providers.FirstOrDefault())
                .Subscribe(change => SelectedProvider = change);

            Activator = new ViewModelActivator();
            this.WhenActivated(disposable =>
            {
                _load.Execute()
                    .Subscribe(x => { })
                    .DisposeWith(disposable);
            });
        }
        
        public ReadOnlyObservableCollection<IProviderViewModel> Providers => _providers;

        [Reactive] public IProviderViewModel SelectedProvider { get; set; }

        public ViewModelActivator Activator { get; }
        
        public bool IsLoading => _isLoading.Value;
        
        public ICommand LoadProviders => _load;

        public bool IsReady => _isReady.Value;
    }
}