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
    public delegate IRenameFileViewModel RenameFileViewModelFactory(IProviderViewModel providerViewModel);

    public sealed class RenameFileViewModel : ReactiveObject, IRenameFileViewModel
    {
        private readonly ObservableAsPropertyHelper<string> _errorMessage;
        private readonly ObservableAsPropertyHelper<string> _oldName;
        private readonly ObservableAsPropertyHelper<bool> _isLoading;
        private readonly ObservableAsPropertyHelper<bool> _hasErrors;
        private readonly ReactiveCommand<Unit, Unit> _rename;
        private readonly ReactiveCommand<Unit, Unit> _close;
        private readonly ReactiveCommand<Unit, Unit> _open;
        
        public RenameFileViewModel(
            IProviderViewModel providerViewModel,
            IProvider provider,
            IScheduler current,
            IScheduler main)
        {
            _oldName = providerViewModel
                .WhenAnyValue(x => x.SelectedFile)
                .Select(file => file?.Name)
                .ToProperty(this, x => x.OldName, scheduler: current);

            var canInteract = providerViewModel
                .WhenAnyValue(x => x.CanInteract);
            
            var oldNameValid = this
                .WhenAnyValue(x => x.OldName)
                .Select(old => !string.IsNullOrWhiteSpace(old));
            
            var canOpen = this
                .WhenAnyValue(x => x.IsVisible)
                .Select(visible => !visible)
                .CombineLatest(oldNameValid, (visible, old) => visible && old)
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

            var canRename = this
                .WhenAnyValue(x => x.NewName)
                .Select(name => !string.IsNullOrWhiteSpace(name))
                .CombineLatest(oldNameValid, (old, name) => old && name);
            
            _rename = ReactiveCommand.CreateFromTask(
                () => provider.RenameFile(providerViewModel.SelectedFile.Path, NewName),
                canRename, main);

            _isLoading = _rename
                .IsExecuting
                .ToProperty(this, x => x.IsLoading, scheduler: current);

            _hasErrors = _rename
                .ThrownExceptions
                .Select(exception => true)
                .Merge(_close.Select(x => false))
                .ToProperty(this, x => x.HasErrors, scheduler: current);

            _errorMessage = _rename
                .ThrownExceptions
                .Select(exception => exception.Message)
                .Log(this, $"Rename file error occured in {provider.Name} for {OldName}")
                .Merge(_close.Select(x => string.Empty))
                .ToProperty(this, x => x.ErrorMessage, scheduler: current);

            _rename.InvokeCommand(_close);
            _close.Subscribe(x => NewName = string.Empty);
        }
        
        [Reactive] public bool IsVisible { get; set; }
        
        [Reactive] public string NewName { get; set; }

        public string ErrorMessage => _errorMessage.Value;

        public bool HasErrors => _hasErrors.Value;

        public bool IsLoading => _isLoading.Value;

        public string OldName => _oldName.Value;

        public ICommand Rename => _rename;

        public ICommand Close => _close;

        public ICommand Open => _open;
    }
}