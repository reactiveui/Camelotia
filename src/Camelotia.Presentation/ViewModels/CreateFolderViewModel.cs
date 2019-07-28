using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;

namespace Camelotia.Presentation.ViewModels
{
    public delegate ICreateFolderViewModel CreateFolderViewModelFactory(IProviderViewModel providerViewModel);

    public sealed class CreateFolderViewModel : ReactiveObject, ICreateFolderViewModel
    {
        private readonly ObservableAsPropertyHelper<string> _errorMessage;
        private readonly ObservableAsPropertyHelper<bool> _isLoading;
        private readonly ObservableAsPropertyHelper<bool> _hasErrors;
        private readonly ObservableAsPropertyHelper<string> _path;
        private readonly ReactiveCommand<Unit, Unit> _create;
        private readonly ReactiveCommand<Unit, Unit> _close;
        private readonly ReactiveCommand<Unit, Unit> _open;
        
        public CreateFolderViewModel(
            IProviderViewModel providerViewModel,
            IProvider provider,
            IScheduler current,
            IScheduler main)
        {
            _path = providerViewModel
                .WhenAnyValue(x => x.CurrentPath)
                .ToProperty(this, x => x.Path, scheduler: current);
            
            var canInteract = providerViewModel
                .WhenAnyValue(x => x.CanInteract);
            
            var pathValid = this
                .WhenAnyValue(x => x.Path)
                .Select(path => !string.IsNullOrWhiteSpace(path));
            
            var canCreateFolder = this
                .WhenAnyValue(x => x.Name)
                .Select(name => !string.IsNullOrWhiteSpace(name))
                .CombineLatest(pathValid, (name, path) => name && path);
            
            _create = ReactiveCommand.CreateFromTask(
                () => provider.CreateFolder(Path, Name),
                canCreateFolder, main);

            var canCreate = Observable.Return(provider.CanCreateFolder);
            var canOpen = this
                .WhenAnyValue(x => x.IsVisible)
                .Select(visible => !visible)
                .CombineLatest(canCreate, pathValid, (visible, can, path) => visible && path && can)
                .CombineLatest(canInteract, (open, interact) => open && interact);
            
            _open = ReactiveCommand.Create(
                () => { IsVisible = true; },
                canOpen, main);

            var canClose = this
                .WhenAnyValue(x => x.IsVisible)
                .Select(visible => visible);
            
            _close = ReactiveCommand.Create(
                () => { IsVisible = false; },
                canClose, main);

            _create.InvokeCommand(_close);
            _close.Subscribe(x => Name = string.Empty);

            _hasErrors = _create
                .ThrownExceptions
                .Select(exception => true)
                .Merge(_close.Select(unit => false))
                .ToProperty(this, x => x.HasErrors, scheduler: current);

            _errorMessage = _create
                .ThrownExceptions
                .Select(exception => exception.Message)
                .Log(this, $"Create folder error occured in {provider.Name}")
                .Merge(_close.Select(unit => string.Empty))
                .ToProperty(this, x => x.ErrorMessage, scheduler: current);

            _isLoading = _create
                .IsExecuting
                .ToProperty(this, x => x.IsLoading, scheduler: current);
        }

        [Reactive] public string Name { get; set; }
        
        [Reactive] public bool IsVisible { get; set; }

        public string ErrorMessage => _errorMessage.Value;

        public bool HasErrors => _hasErrors.Value;

        public bool IsLoading => _isLoading.Value;

        public string Path => _path.Value;
        
        public ICommand Create => _create;
        
        public ICommand Close => _close;

        public ICommand Open => _open;
    }
}