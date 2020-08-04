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
        private readonly ObservableAsPropertyHelper<bool> _hasErrorMessage;
        private readonly ObservableAsPropertyHelper<string> _errorMessage;
        private readonly ObservableAsPropertyHelper<bool> _isLoading;
        private readonly ObservableAsPropertyHelper<string> _path;
        private readonly ReactiveCommand<Unit, Unit> _create;
        private readonly ReactiveCommand<Unit, Unit> _close;
        private readonly ReactiveCommand<Unit, Unit> _open;
        
        public CreateFolderViewModel(
            IProviderViewModel owner,
            IProvider provider)
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
            _close.Subscribe(x => Name = string.Empty);

            _hasErrorMessage = _create
                .ThrownExceptions
                .Select(exception => true)
                .Merge(_close.Select(unit => false))
                .ToProperty(this, x => x.HasErrorMessage);

            _errorMessage = _create
                .ThrownExceptions
                .Select(exception => exception.Message)
                .Log(this, $"Create folder error occured in {provider.Name}")
                .Merge(_close.Select(unit => string.Empty))
                .ToProperty(this, x => x.ErrorMessage);

            _isLoading = _create
                .IsExecuting
                .ToProperty(this, x => x.IsLoading);
        }

        [Reactive] public string Name { get; set; }
        
        [Reactive] public bool IsVisible { get; set; }

        public string ErrorMessage => _errorMessage.Value;

        public bool HasErrorMessage => _hasErrorMessage.Value;

        public bool IsLoading => _isLoading.Value;

        public string Path => _path.Value;
        
        public ICommand Create => _create;
        
        public ICommand Close => _close;

        public ICommand Open => _open;
    }
}