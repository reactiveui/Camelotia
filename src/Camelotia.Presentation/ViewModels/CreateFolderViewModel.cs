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
    public delegate ICreateFolderViewModel CreateFolderViewModelFactory(IProviderViewModel providerViewModel);

    public sealed class CreateFolderViewModel : ReactiveValidationObject<CreateFolderViewModel>, ICreateFolderViewModel
    {
        public CreateFolderViewModel(CreateFolderState state, IProviderViewModel owner, IProvider provider)
        {
            owner.WhenAnyValue(x => x.CurrentPath)
                 .ToPropertyEx(this, x => x.Path);
            
            this.ValidationRule(x => x.Name,
                name => !string.IsNullOrWhiteSpace(name),
                "Folder name shouldn't be empty.");

            var pathRule = this.ValidationRule(x => x.Path,
                path => !string.IsNullOrWhiteSpace(path),
                "Path shouldn't be empty");
            
            Create = ReactiveCommand.CreateFromTask(
                () => provider.CreateFolder(Path, Name),
                this.IsValid());
            
            Create.IsExecuting.ToPropertyEx(this, x => x.IsLoading);

            var canInteract = owner
                .WhenAnyValue(x => x.CanInteract)
                .Skip(1)
                .StartWith(true);
            
            var canOpen = this
                .WhenAnyValue(x => x.IsVisible)
                .Select(visible => !visible)
                .CombineLatest(
                    canInteract,
                    pathRule.WhenAnyValue(x => x.IsValid), 
                    (visible, interact, path) => visible && provider.CanCreateFolder && interact && path);
            
            var canClose = this
                .WhenAnyValue(x => x.IsVisible)
                .Select(visible => visible);
            
            Open = ReactiveCommand.Create(() => {}, canOpen);
            Close = ReactiveCommand.Create(() => {}, canClose);
            
            Open.Select(unit => true)
                .Merge(Close.Select(unit => false))
                .Subscribe(visible => IsVisible = visible);
            
            Close.Subscribe(x => Name = string.Empty);
            Create.InvokeCommand(Close);

            Create.ThrownExceptions
                .Select(exception => true)
                .Merge(Close.Select(unit => false))
                .ToPropertyEx(this, x => x.HasErrorMessage);

            Create.ThrownExceptions
                .Select(exception => exception.Message)
                .Log(this, $"Create folder error occured in {provider.Name}")
                .Merge(Close.Select(unit => string.Empty))
                .ToPropertyEx(this, x => x.ErrorMessage);

            Name = state.Name;
            IsVisible = state.IsVisible;

            this.WhenAnyValue(x => x.Name)
                .Subscribe(name => state.Name = name);
            this.WhenAnyValue(x => x.IsVisible)
                .Subscribe(visible => state.IsVisible = visible);
        }

        [Reactive]
        public string Name { get; set; }
        
        [Reactive]
        public bool IsVisible { get; set; }

        [ObservableAsProperty]
        public string ErrorMessage { get; }

        [ObservableAsProperty]
        public bool HasErrorMessage { get; }

        [ObservableAsProperty]
        public bool IsLoading { get; }

        [ObservableAsProperty]
        public string Path { get; }
        
        public ReactiveCommand<Unit, Unit> Create { get; }

        public ReactiveCommand<Unit, Unit> Close { get; }

        public ReactiveCommand<Unit, Unit> Open { get; }
    }
}