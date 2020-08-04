using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace Camelotia.Presentation.ViewModels
{
    public sealed class DirectAuthViewModel : ReactiveValidationObject<DirectAuthViewModel>, IDirectAuthViewModel
    {
        private readonly ObservableAsPropertyHelper<bool> _hasErrorMessage;
        private readonly ObservableAsPropertyHelper<string> _errorMessage;
        private readonly ObservableAsPropertyHelper<bool> _isBusy;
        private readonly ReactiveCommand<Unit, Unit> _login;
        
        public DirectAuthViewModel(IProvider provider)
        {
            this.ValidationRule(x => x.Username,
                name => !string.IsNullOrWhiteSpace(name),
                "User name shouldn't be null or white space.");

            this.ValidationRule(x => x.Password,
                pass => !string.IsNullOrWhiteSpace(pass),
                "Password shouldn't be null or white space.");
            
            _login = ReactiveCommand.CreateFromTask(
                () => provider.DirectAuth(Username, Password),
                this.IsValid());

            _errorMessage = _login
                .ThrownExceptions
                .Select(exception => exception.Message)
                .Log(this, $"Direct auth error occured in {provider.Name}")
                .ToProperty(this, x => x.ErrorMessage);

            _hasErrorMessage = _login
                .ThrownExceptions
                .Select(exception => true)
                .Merge(_login.Select(unit => false))
                .ToProperty(this, x => x.HasErrorMessage);

            _isBusy = _login
                .IsExecuting
                .ToProperty(this, x => x.IsBusy);

            _login.Subscribe(x =>
            {
                Username = string.Empty;
                Password = string.Empty;
            });
        }
        
        [Reactive] public string Username { get; set; }
        
        [Reactive] public string Password { get; set; }
        
        public string ErrorMessage => _errorMessage.Value;

        public bool HasErrorMessage => _hasErrorMessage.Value;

        public bool IsBusy => _isBusy.Value;
        
        public ICommand Login => _login;
    }
}