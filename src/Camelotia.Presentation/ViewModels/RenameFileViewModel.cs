using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace Camelotia.Presentation.ViewModels
{
    public delegate IRenameFileViewModel RenameFileViewModelFactory(IProviderViewModel providerViewModel);

    public sealed class RenameFileViewModel : ReactiveValidationObject<RenameFileViewModel>, IRenameFileViewModel
    {
        private readonly ObservableAsPropertyHelper<bool> _hasErrorMessage;
        private readonly ObservableAsPropertyHelper<string> _errorMessage;
        private readonly ObservableAsPropertyHelper<string> _oldName;
        private readonly ObservableAsPropertyHelper<bool> _isLoading;
        private readonly ReactiveCommand<Unit, Unit> _rename;
        private readonly ReactiveCommand<Unit, Unit> _close;
        private readonly ReactiveCommand<Unit, Unit> _open;
        
        public RenameFileViewModel(IProviderViewModel owner, IProvider provider)
        {
            _oldName = owner
                .WhenAnyValue(x => x.SelectedFile)
                .Select(file => file?.Name)
                .ToProperty(this, x => x.OldName);
            
            this.ValidationRule(x => x.NewName,
                name => !string.IsNullOrWhiteSpace(name),
                "New name shouldn't be empty.");

            var oldRule = this.ValidationRule(x => x.OldName,
                name => !string.IsNullOrWhiteSpace(name),
                "Old name shouldn't be empty.");
            
            _rename = ReactiveCommand.CreateFromTask(
                () => provider.RenameFile(owner.SelectedFile.Path, NewName),
                this.IsValid());

            var canOpen = this
                .WhenAnyValue(x => x.IsVisible)
                .Select(visible => !visible)
                .CombineLatest(
                    oldRule.WhenAnyValue(x => x.IsValid), 
                    owner.WhenAnyValue(x => x.CanInteract), 
                    (visible, old, interact) => visible && old && interact);
            
            _open = ReactiveCommand.Create(
                () => { IsVisible = true; },
                canOpen);
            
            var canClose = this
                .WhenAnyValue(x => x.IsVisible)
                .Select(visible => visible);
            
            _close = ReactiveCommand.Create(
                () => { IsVisible = false; },
                canClose);

            _isLoading = _rename
                .IsExecuting
                .ToProperty(this, x => x.IsLoading);

            _hasErrorMessage = _rename
                .ThrownExceptions
                .Select(exception => true)
                .Merge(_close.Select(x => false))
                .ToProperty(this, x => x.HasErrorMessage);

            _errorMessage = _rename
                .ThrownExceptions
                .Select(exception => exception.Message)
                .Log(this, $"Rename file error occured in {provider.Name} for {OldName}")
                .Merge(_close.Select(x => string.Empty))
                .ToProperty(this, x => x.ErrorMessage);

            _rename.InvokeCommand(_close);
            _close.Subscribe(x => NewName = string.Empty);
        }
        
        [Reactive] public bool IsVisible { get; set; }
        
        [Reactive] public string NewName { get; set; }

        public string ErrorMessage => _errorMessage.Value;

        public bool HasErrorMessage => _hasErrorMessage.Value;

        public bool IsLoading => _isLoading.Value;

        public string OldName => _oldName.Value;

        public ICommand Rename => _rename;

        public ICommand Close => _close;

        public ICommand Open => _open;
    }
}