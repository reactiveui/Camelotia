using System;
using System.Reactive;
using System.Reactive.Concurrency;
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
        
        public DirectAuthViewModel(
            IProvider provider,
            IScheduler current,
            IScheduler main)
        {
            var canLogin = this
                .WhenAnyValue(
                    x => x.Username,
                    x => x.Password,
                    (name, pass) =>
                        !string.IsNullOrWhiteSpace(name) &&
                        !string.IsNullOrWhiteSpace(pass))
                .DistinctUntilChanged();

            _login = ReactiveCommand.CreateFromTask(
                () => provider.DirectAuth(Username, Password),
                canLogin, main);

            _errorMessage = _login
                .ThrownExceptions
                .Select(exception => exception.Message)
                .Log(this, $"Direct auth error occured in {provider.Name}")
                .ToProperty(this, x => x.ErrorMessage, scheduler: current);

            _hasErrors = _login
                .ThrownExceptions
                .Select(exception => true)
                .Merge(_login.Select(unit => false))
                .ToProperty(this, x => x.HasErrors, scheduler: current);

            _isBusy = _login
                .IsExecuting
                .ToProperty(this, x => x.IsBusy, scheduler: current);

            _login.Subscribe(x =>
            {
                Username = string.Empty;
                Password = string.Empty;
            });
        }
        
        [Reactive] public string Username { get; set; }
        
        [Reactive] public string Password { get; set; }
        
        public string ErrorMessage => _errorMessage.Value;

        public bool HasErrors => _hasErrors.Value;

        public bool IsBusy => _isBusy.Value;
        
        public ICommand Login => _login;
    }
}