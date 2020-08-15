using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Extensions;
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
        private readonly ReactiveCommand<Unit, Unit> _create;
        private readonly ReactiveCommand<Unit, bool> _close;
        private readonly ReactiveCommand<Unit, bool> _open;
        
        public CreateFolderViewModel(CreateFolderState state, IProviderViewModel owner, IProvider provider)
        {
            owner.WhenAnyValue(x => x.CurrentPath).ToPropertyEx(this, x => x.Path);
            
            this.ValidationRule(x => x.Name,
                name => !string.IsNullOrWhiteSpace(name),
                "Folder name shouldn't be empty.");

            var pathRule = this.ValidationRule(x => x.Path,
                path => !string.IsNullOrWhiteSpace(path),
                "Path shouldn't be empty");
            
            _create = ReactiveCommand.CreateFromTask(
                () => provider.CreateFolder(Path, Name),
                this.IsValid());
            
            _create.IsExecuting.ToPropertyEx(this, x => x.IsLoading);

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
            
            _open = ReactiveCommand.Create(() => true, canOpen);
            _close = ReactiveCommand.Create(() => false, canClose);
            
            _close.Merge(_open).Subscribe(visible => IsVisible = visible);
            _close.Subscribe(x => Name = string.Empty);
            
            _create.InvokeCommand(_close);

            _create.ThrownExceptions
                .Select(exception => true)
                .Merge(_close.Select(unit => false))
                .ToPropertyEx(this, x => x.HasErrorMessage);

            _create.ThrownExceptions
                .Select(exception => exception.Message)
                .Log(this, $"Create folder error occured in {provider.Name}")
                .Merge(_close.Select(unit => string.Empty))
                .ToPropertyEx(this, x => x.ErrorMessage);

            this.AutoUpdate(x => x.Name, state, x => x.Name);
            this.AutoUpdate(x => x.IsVisible, state, x => x.IsVisible);
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
        
        public ICommand Create => _create;
        
        public ICommand Close => _close;

        public ICommand Open => _open;
    }
}