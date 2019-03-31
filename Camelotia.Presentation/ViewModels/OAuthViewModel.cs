using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.ViewModels
{
    public sealed class OAuthViewModel : ReactiveObject, IOAuthViewModel
    {
        private readonly ObservableAsPropertyHelper<string> _errorMessage;
        private readonly ObservableAsPropertyHelper<bool> _hasErrors;
        private readonly ObservableAsPropertyHelper<bool> _isBusy;
        private readonly ReactiveCommand<Unit, Unit> _login;
        
        public OAuthViewModel(
            IScheduler currentThread,
            IScheduler mainThread,
            IProvider provider)
        {
            _login = ReactiveCommand.CreateFromTask(
                provider.OAuth,
                outputScheduler: mainThread);

            _errorMessage = _login
                .ThrownExceptions
                .Select(exception => exception.Message)
                .Log(this, $"OAuth error occured in {provider.Name}")
                .ToProperty(this, x => x.ErrorMessage, scheduler: currentThread);

            _hasErrors = _login
                .ThrownExceptions
                .Select(exception => true)
                .Merge(_login.Select(unit => false))
                .ToProperty(this, x => x.HasErrors, scheduler: currentThread);
            
            _isBusy = _login
                .IsExecuting
                .ToProperty(this, x => x.IsBusy, scheduler: currentThread);
        }
        
        public string ErrorMessage => _errorMessage.Value;

        public bool HasErrors => _hasErrors.Value;
        
        public bool IsBusy => _isBusy.Value;
        
        public ICommand Login => _login;
    }
}