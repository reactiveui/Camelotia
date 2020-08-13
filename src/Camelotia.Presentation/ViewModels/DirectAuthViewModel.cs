using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Extensions;
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
        private readonly ReactiveCommand<Unit, Unit> _login;
        
        public DirectAuthViewModel(DirectAuthState state, IProvider provider)
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
            
            _login.IsExecuting.ToPropertyEx(this, x => x.IsBusy);
            
            _login.ThrownExceptions
                .Select(exception => exception.Message)
                .Log(this, $"Direct auth error occured in {provider.Name}")
                .ToPropertyEx(this, x => x.ErrorMessage);

            _login.ThrownExceptions
                .Select(exception => true)
                .Merge(_login.Select(unit => false))
                .ToPropertyEx(this, x => x.HasErrorMessage);

            _login.Subscribe(x =>
            {
                Username = string.Empty;
                Password = string.Empty;
            });

            this.AutoUpdate(state, x => x.Username, x => x.Username);
            this.AutoUpdate(state, x => x.Password, x => x.Password);
        }
        
        [Reactive]
        public string Username { get; set; }
        
        [Reactive]
        public string Password { get; set; }
        
        [ObservableAsProperty]
        public string ErrorMessage { get; }

        [ObservableAsProperty]
        public bool HasErrorMessage { get; }

        [ObservableAsProperty]
        public bool IsBusy { get; }
        
        public ICommand Login => _login;
    }
}