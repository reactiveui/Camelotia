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
        private readonly ReactiveCommand<Unit, Unit> _rename;
        private readonly ReactiveCommand<Unit, bool> _close;
        private readonly ReactiveCommand<Unit, bool> _open;
        
        public RenameFileViewModel(IProviderViewModel owner, IProvider provider)
        {
            owner.WhenAnyValue(x => x.SelectedFile)
                .Select(file => file?.Name)
                .ToPropertyEx(this, x => x.OldName);
            
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

            var canOpen = this
                .WhenAnyValue(x => x.IsVisible)
                .Select(visible => !visible)
                .CombineLatest(
                    oldRule.WhenAnyValue(x => x.IsValid), 
                    owner.WhenAnyValue(x => x.CanInteract), 
                    (visible, old, interact) => visible && old && interact);
            
            var canClose = this
                .WhenAnyValue(x => x.IsVisible)
                .Select(visible => visible);
            
            _open = ReactiveCommand.Create(() => true, canOpen);
            _close = ReactiveCommand.Create(() => false, canClose);
            
            _close.Merge(_open).Subscribe(visible => IsVisible = visible);
            _close.Subscribe(x => NewName = string.Empty);

            _rename.ThrownExceptions
                .Select(exception => true)
                .Merge(_close.Select(x => false))
                .ToPropertyEx(this, x => x.HasErrorMessage);

            _rename.ThrownExceptions
                .Select(exception => exception.Message)
                .Log(this, $"Rename file error occured in {provider.Name} for {OldName}")
                .Merge(_close.Select(x => string.Empty))
                .ToPropertyEx(this, x => x.ErrorMessage);

            _rename.InvokeCommand(_close);
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