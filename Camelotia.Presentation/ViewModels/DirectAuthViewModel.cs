using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Camelotia.Presentation.ViewModels
{
    public sealed class DirectAuthViewModel : ReactiveObject, IDirectAuthViewModel
    {
        private readonly ObservableAsPropertyHelper<string> _errorMessage;
        private readonly ObservableAsPropertyHelper<bool> _hasErrors;
        private readonly ObservableAsPropertyHelper<bool> _isBusy;
        private readonly ReactiveCommand<Unit, Unit> _login;
        
        public DirectAuthViewModel(IProvider provider)
        {
            var main = RxApp.MainThreadScheduler;
            var nameValid = this
                .WhenAnyValue(x => x.Username)
                .Select(name => !string.IsNullOrWhiteSpace(name));

            var passwordValid = this
                .WhenAnyValue(x => x.Password)
                .Select(name => !string.IsNullOrWhiteSpace(name));

            var canLogin = nameValid
                .CombineLatest(passwordValid, (name, password) => name && password)
                .DistinctUntilChanged();
            
            _login = ReactiveCommand.CreateFromTask(
                () => provider.DirectAuth(Username, Password),
                canLogin);

            _errorMessage = _login
                .ThrownExceptions
                .Select(exception => exception.Message)
                .ToProperty(this, x => x.ErrorMessage, scheduler: main);

            _hasErrors = _login
                .ThrownExceptions
                .Select(exception => true)
                .Merge(_login.Select(unit => false))
                .ToProperty(this, x => x.HasErrors, scheduler: main);

            _isBusy = _login
                .IsExecuting
                .ToProperty(this, x => x.IsBusy, scheduler: main);
            
            _login.Subscribe(x => Username = string.Empty);
            _login.Subscribe(x => Password = string.Empty);            
        }
        
        [Reactive] public string Username { get; set; }
        
        [Reactive] public string Password { get; set; }
        
        public string ErrorMessage => _errorMessage.Value;

        public bool HasErrors => _hasErrors.Value;

        public bool IsBusy => _isBusy.Value;
        
        public ICommand Login => _login;
    }
}