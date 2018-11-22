using System.Reactive;
using System.Reactive.Linq;
using System.Security.Cryptography.X509Certificates;
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
        
        public OAuthViewModel(IProvider provider)
        {
            Activator = new ViewModelActivator();
            _login = ReactiveCommand.CreateFromTask(provider.OAuth);

            _errorMessage = _login.ThrownExceptions
                .Select(exception => exception.Message)
                .ToProperty(this, x => x.ErrorMessage);

            _hasErrors = _login.ThrownExceptions
                .Select(exception => true)
                .Merge(_login.Select(unit => false))
                .ToProperty(this, x => x.HasErrors);
            
            _isBusy = _login.IsExecuting
                .ToProperty(this, x => x.IsBusy);
        }
        
        public ViewModelActivator Activator { get; }

        public string ErrorMessage => _errorMessage.Value;

        public bool HasErrors => _hasErrors.Value;
        
        public bool IsBusy => _isBusy.Value;
        
        public ICommand Login => _login;
    }
}