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

    public sealed class CreateFolderViewModel : ReactiveValidationObject, ICreateFolderViewModel
    {
        private readonly ObservableAsPropertyHelper<string> _errorMessage;
        private readonly ObservableAsPropertyHelper<bool> _hasErrorMessage;
        private readonly ObservableAsPropertyHelper<bool> _isLoading;
        private readonly ObservableAsPropertyHelper<string> _path;
        
        public CreateFolderViewModel(CreateFolderState state, IProviderViewModel owner, IProvider provider)
        {
            _path = owner
                .WhenAnyValue(x => x.CurrentPath)
                .ToProperty(this, x => x.Path);
            
            this.ValidationRule(x => x.Name,
                name => !string.IsNullOrWhiteSpace(name),
                "Folder name shouldn't be empty.");

            var pathRule = this.ValidationRule(x => x.Path,
                path => !string.IsNullOrWhiteSpace(path),
                "Path shouldn't be empty");
            
            Create = ReactiveCommand.CreateFromTask(
                () => provider.CreateFolder(Path, Name),
                this.IsValid());
            
            _isLoading = Create.IsExecuting
                .ToProperty(this, x => x.IsLoading);

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

            _hasErrorMessage = Create
                .ThrownExceptions
                .Select(exception => true)
                .Merge(Close.Select(unit => false))
                .ToProperty(this, x => x.HasErrorMessage);

            _errorMessage = Create
                .ThrownExceptions
                .Select(exception => exception.Message)
                .Log(this, $"Create folder error occured in {provider.Name}")
                .Merge(Close.Select(unit => string.Empty))
                .ToProperty(this, x => x.ErrorMessage);

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

        public string ErrorMessage => _errorMessage.Value;

        public bool HasErrorMessage => _hasErrorMessage.Value;

        public bool IsLoading => _isLoading.Value;

        public string Path => _path.Value;
        
        public ReactiveCommand<Unit, Unit> Create { get; }

        public ReactiveCommand<Unit, Unit> Close { get; }

        public ReactiveCommand<Unit, Unit> Open { get; }
    }
}