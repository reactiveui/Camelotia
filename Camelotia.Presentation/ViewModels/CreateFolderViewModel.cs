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
            IScheduler mainThread,
            IProvider provider)
        {
            _path = providerViewModel
                .WhenAnyValue(x => x.CurrentPath)
                .ToProperty(this, x => x.Path);
            
            var pathValid = this
                .WhenAnyValue(x => x.Path)
                .Select(path => !string.IsNullOrWhiteSpace(path));
            
            var canCreateFolder = this
                .WhenAnyValue(x => x.Name)
                .Select(name => !string.IsNullOrWhiteSpace(name))
                .CombineLatest(pathValid, (name, path) => name && path);
            
            _create = ReactiveCommand.CreateFromTask(
                () => provider.CreateFolder(Path, Name),
                canCreateFolder, mainThread);
            
            var canCreate = Observable.Return(provider.CanCreateFolder);
            var canOpen = this
                .WhenAnyValue(x => x.IsVisible)
                .Select(visible => !visible)
                .CombineLatest(canCreate, (visible, can) => visible && can)
                .CombineLatest(pathValid, (can, path) => can && path);
            
            _open = ReactiveCommand.Create(
                () => { IsVisible = true; },
                canOpen, mainThread);

            var canClose = this
                .WhenAnyValue(x => x.IsVisible)
                .Select(visible => visible);
            
            _close = ReactiveCommand.Create(
                () => { IsVisible = false; },
                canClose, mainThread);

            _hasErrors = _create
                .ThrownExceptions
                .Select(exception => true)
                .Merge(_close.Select(unit => false))
                .ToProperty(this, x => x.HasErrors);

            _errorMessage = _create
                .ThrownExceptions
                .Select(exception => exception.Message)
                .Merge(_close.Select(unit => string.Empty))
                .ToProperty(this, x => x.ErrorMessage);

            _isLoading = _create.IsExecuting
                .ToProperty(this, x => x.IsLoading);

            _close.Subscribe(x => Name = string.Empty);
            _create.InvokeCommand(Close);
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