using System;
using System.Reactive;
using System.Reactive.Linq;
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
        public RenameFileViewModel(RenameFileState state, IProviderViewModel owner, IProvider provider)
        {
            owner.WhenAnyValue(x => x.SelectedFile.Name)
                 .ToPropertyEx(this, x => x.OldName);
            
            this.ValidationRule(x => x.NewName,
                name => !string.IsNullOrWhiteSpace(name),
                "New name shouldn't be empty.");

            var oldRule = this.ValidationRule(x => x.OldName,
                name => !string.IsNullOrWhiteSpace(name),
                "Old name shouldn't be empty.");
            
            Rename = ReactiveCommand.CreateFromTask(
                () => provider.RenameFile(owner.SelectedFile.Path, NewName),
                this.IsValid());
            
            Rename.IsExecuting.ToPropertyEx(this, x => x.IsLoading);

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
            
            Open = ReactiveCommand.Create(() => {}, canOpen);
            Close = ReactiveCommand.Create(() => {}, canClose);
            
            Open.Select(unit => true)
                .Merge(Close.Select(unit => false))
                .Subscribe(visible => IsVisible = visible);
            
            Close.Subscribe(x => NewName = string.Empty);
            Rename.InvokeCommand(Close);
            
            Rename.ThrownExceptions
                .Select(exception => true)
                .Merge(Close.Select(x => false))
                .ToPropertyEx(this, x => x.HasErrorMessage);

            Rename.ThrownExceptions
                .Select(exception => exception.Message)
                .Log(this, $"Rename file error occured in {provider.Name} for {OldName}")
                .Merge(Close.Select(x => string.Empty))
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

        public ReactiveCommand<Unit, Unit> Rename { get; }

        public ReactiveCommand<Unit, Unit> Close { get; }

        public ReactiveCommand<Unit, Unit> Open { get; }
    }
}