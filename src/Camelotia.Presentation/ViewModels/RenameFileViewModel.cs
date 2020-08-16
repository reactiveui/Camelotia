using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Camelotia.Presentation.AppState;
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
        private readonly ReactiveCommand<Unit, Unit> _rename;
        private readonly ReactiveCommand<Unit, bool> _close;
        private readonly ReactiveCommand<Unit, bool> _open;
        
        public RenameFileViewModel(RenameFileState state, IProviderViewModel owner, IProvider provider)
        {
            owner.WhenAnyValue(x => x.SelectedFile.Name).ToPropertyEx(this, x => x.OldName);
            
            this.ValidationRule(x => x.NewName,
                name => !string.IsNullOrWhiteSpace(name),
                "New name shouldn't be empty.");

            var oldRule = this.ValidationRule(x => x.OldName,
                name => !string.IsNullOrWhiteSpace(name),
                "Old name shouldn't be empty.");
            
            _rename = ReactiveCommand.CreateFromTask(
                () => provider.RenameFile(owner.SelectedFile.Path, NewName),
                this.IsValid());
            
            _rename.IsExecuting.ToPropertyEx(this, x => x.IsLoading);

            var canInteract = owner
                .WhenAnyValue(x => x.CanInteract)
                .Skip(1)
                .StartWith(true);
            
            var canOpen = this
                .WhenAnyValue(x => x.IsVisible)
                .Select(visible => !visible)
                .CombineLatest(
                    canInteract,
                    oldRule.WhenAnyValue(x => x.IsValid), 
                    (visible, interact, old) => visible && old && interact);
            
            var canClose = this
                .WhenAnyValue(x => x.IsVisible)
                .Select(visible => visible);
            
            _open = ReactiveCommand.Create(() => true, canOpen);
            _close = ReactiveCommand.Create(() => false, canClose);
            
            _close.Merge(_open).Subscribe(visible => IsVisible = visible);
            _close.Subscribe(x => NewName = string.Empty);

            _rename.InvokeCommand(_close);
            
            _rename.ThrownExceptions
                .Select(exception => true)
                .Merge(_close.Select(x => false))
                .ToPropertyEx(this, x => x.HasErrorMessage);

            _rename.ThrownExceptions
                .Select(exception => exception.Message)
                .Log(this, $"Rename file error occured in {provider.Name} for {OldName}")
                .Merge(_close.Select(x => string.Empty))
                .ToPropertyEx(this, x => x.ErrorMessage);

            NewName = state.NewName;
            this.WhenAnyValue(x => x.NewName)
                .Subscribe(name => state.NewName = name);
        }
        
        [Reactive] 
        public bool IsVisible { get; set; }
        
        [Reactive] 
        public string NewName { get; set; }

        [ObservableAsProperty]
        public bool HasErrorMessage { get; }

        [ObservableAsProperty]
        public string ErrorMessage { get; }

        [ObservableAsProperty]
        public bool IsLoading { get; }

        [ObservableAsProperty]
        public string OldName { get; }

        public ICommand Rename => _rename;

        public ICommand Close => _close;

        public ICommand Open => _open;
    }
}