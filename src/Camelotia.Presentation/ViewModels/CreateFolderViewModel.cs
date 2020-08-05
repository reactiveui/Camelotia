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
    public delegate ICreateFolderViewModel CreateFolderViewModelFactory(IProviderViewModel providerViewModel);

    public sealed class CreateFolderViewModel : ReactiveValidationObject<CreateFolderViewModel>, ICreateFolderViewModel
    {
        private readonly ReactiveCommand<Unit, Unit> _create;
        private readonly ReactiveCommand<Unit, Unit> _close;
        private readonly ReactiveCommand<Unit, Unit> _open;
        
        public CreateFolderViewModel(IProviderViewModel owner, IProvider provider)
        {
            owner.WhenAnyValue(x => x.CurrentPath)
                .ToPropertyEx(this, x => x.Path);
            
            this.ValidationRule(x => x.Name,
                name => !string.IsNullOrWhiteSpace(name),
                "Folder name shouldn't be empty.");

            var pathRule = this.ValidationRule(x => x.Path,
                path => !string.IsNullOrWhiteSpace(path),
                "Path shouldn't be empty");
            
            _create = ReactiveCommand.CreateFromTask(
                () => provider.CreateFolder(Path, Name),
                this.IsValid());
            
            var canOpen = this
                .WhenAnyValue(x => x.IsVisible)
                .Select(visible => !visible)
                .CombineLatest(
                    owner.WhenAnyValue(x => x.CanInteract),
                    pathRule.WhenAnyValue(x => x.IsValid), 
                    (visible, interact, path) => visible && provider.CanCreateFolder && interact && path);
            
            _open = ReactiveCommand.Create(
                () => { IsVisible = true; },
                canOpen);

            var canClose = this
                .WhenAnyValue(x => x.IsVisible)
                .Select(visible => visible);
            
            _close = ReactiveCommand.Create(
                () => { IsVisible = false; },
                canClose);

            _create.InvokeCommand(_close);
            _create.IsExecuting.ToPropertyEx(this, x => x.IsLoading);
            _close.Subscribe(x => Name = string.Empty);

            _create.ThrownExceptions
                .Select(exception => true)
                .Merge(_close.Select(unit => false))
                .ToPropertyEx(this, x => x.HasErrorMessage);

            _create.ThrownExceptions
                .Select(exception => exception.Message)
                .Log(this, $"Create folder error occured in {provider.Name}")
                .Merge(_close.Select(unit => string.Empty))
                .ToPropertyEx(this, x => x.ErrorMessage);
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