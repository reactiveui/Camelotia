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
        private readonly ObservableAsPropertyHelper<bool> _hasErrorMessage;
        private readonly ObservableAsPropertyHelper<string> _errorMessage;
        private readonly ObservableAsPropertyHelper<bool> _isBusy;
        private readonly ReactiveCommand<Unit, Unit> _login;
        
        public OAuthViewModel(IProvider provider)
        {
            _login = ReactiveCommand.CreateFromTask(provider.OAuth);
            _isBusy = _login.IsExecuting.ToProperty(this, x => x.IsBusy);

            _errorMessage = _login.ThrownExceptions
                .Select(exception => exception.Message)
                .Log(this, $"OAuth error occured in {provider.Name}")
                .ToProperty(this, x => x.ErrorMessage);

            _hasErrorMessage = _login.ThrownExceptions
                .Select(exception => true)
                .Merge(_login.Select(unit => false))
                .ToProperty(this, x => x.HasErrorMessage);
        }
        
        public string ErrorMessage => _errorMessage.Value;

        public bool HasErrorMessage => _hasErrorMessage.Value;
        
        public bool IsBusy => _isBusy.Value;
        
        public ICommand Login => _login;
    }
}