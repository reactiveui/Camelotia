using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using System;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace Camelotia.Presentation.ViewModels
{
    public sealed class HostAuthViewModel : ReactiveValidationObject<HostAuthViewModel>, IHostAuthViewModel
    {
        private readonly ReactiveCommand<Unit, Unit> _login;
        
        public HostAuthViewModel(HostAuthState state, IProvider provider)
        {
            this.ValidationRule(x => x.Username,
                name => !string.IsNullOrWhiteSpace(name),
                "User name shouldn't be null or white space.");

            this.ValidationRule(x => x.Password,
                pass => !string.IsNullOrWhiteSpace(pass),
                "Password shouldn't be null or white space.");

            this.ValidationRule(x => x.Address,
                host => !string.IsNullOrWhiteSpace(host),
                "Host address shouldn't be null or white space.");

            this.ValidationRule(x => x.Port,
                port => int.TryParse(port, out _),
                "Port should be a valid integer.");
            
            _login = ReactiveCommand.CreateFromTask(
                () => provider.HostAuth(Address, int.Parse(Port), Username, Password),
                this.IsValid());

            _login.IsExecuting.ToPropertyEx(this, x => x.IsBusy);
            
            _login.ThrownExceptions
                .Select(exception => exception.Message)
                .Log(this, $"Host auth error occured in {provider.Name}")
                .ToPropertyEx(this, x => x.ErrorMessage);

            _login.ThrownExceptions
                .Select(exception => true)
                .Merge(_login.Select(unit => false))
                .ToPropertyEx(this, x => x.HasErrorMessage);

            _login.Subscribe(x =>
            {
                Username = string.Empty;
                Password = string.Empty;
                Address = string.Empty;
                Port = string.Empty;
            });

            Username = state.Username;
            this.WhenAnyValue(x => x.Username)
                .Subscribe(name => state.Username = name);
            
            Password = state.Password;
            this.WhenAnyValue(x => x.Password)
                .Subscribe(name => state.Password = name);
            
            Address = state.Address;
            this.WhenAnyValue(x => x.Address)
                .Subscribe(name => state.Address = name);
            
            Port = state.Port;
            this.WhenAnyValue(x => x.Port)
                .Subscribe(name => state.Port = name);
        }
        
        [Reactive]
        public string Port { get; set; }
        
        [Reactive] 
        public string Address { get; set; }
        
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