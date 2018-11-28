using System;
using System.Collections.Generic;
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

namespace Camelotia.Presentation.ViewModels
{
    public sealed class MainViewModel : ReactiveObject, IMainViewModel
    {
        private readonly ObservableAsPropertyHelper<IEnumerable<IProviderViewModel>> _providers;
        private readonly ReactiveCommand<Unit, IEnumerable<IProvider>> _loadProviders;
        private readonly ObservableAsPropertyHelper<bool> _isLoading;
        private readonly ObservableAsPropertyHelper<bool> _isReady;

        public MainViewModel(
            Func<IProvider, IFileManager, IAuthViewModel, IProviderViewModel> providerFactory,
            Func<IProvider, IAuthViewModel> authFactory,
            IProviderStorage providerStorage, 
            IFileManager fileManager)
        {
            _loadProviders = ReactiveCommand.CreateFromTask(providerStorage.LoadProviders);
            _providers = _loadProviders
                .Select(items => items.Select(x => providerFactory(x, fileManager, authFactory(x))).ToList())
                .StartWithEmpty()
                .ToProperty(this, x => x.Providers);
            
            _isLoading = _loadProviders.IsExecuting
                .ToProperty(this, x => x.IsLoading);
            
            _isReady = _loadProviders.IsExecuting
                .Select(executing => !executing)
                .Skip(1)
                .ToProperty(this, x => x.IsReady);
            
            this.WhenAnyValue(x => x.Providers)
                .Where(providers => providers != null)
                .Select(providers => providers.FirstOrDefault())
                .Subscribe(x => SelectedProvider = x);

            Activator = new ViewModelActivator();
            this.WhenActivated(disposable =>
            {
                _loadProviders.Execute()
                    .Subscribe(x => { })
                    .DisposeWith(disposable);
            });
        }

        [Reactive] public IProviderViewModel SelectedProvider { get; set; }
        
        public IEnumerable<IProviderViewModel> Providers => _providers.Value;
        
        public ICommand LoadProviders => _loadProviders;
        
        public bool IsLoading => _isLoading.Value;

        public bool IsReady => _isReady.Value;

        public ViewModelActivator Activator { get; }
    }
}