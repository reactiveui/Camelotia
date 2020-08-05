using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Camelotia.Presentation.ViewModels
{
    public sealed class OAuthViewModel : ReactiveObject, IOAuthViewModel
    {
        private readonly ReactiveCommand<Unit, Unit> _login;
        
        public OAuthViewModel(IProvider provider)
        {
            _login = ReactiveCommand.CreateFromTask(provider.OAuth);
            _login.IsExecuting.ToPropertyEx(this, x => x.IsBusy);

            _login.ThrownExceptions
                .Select(exception => exception.Message)
                .Log(this, $"OAuth error occured in {provider.Name}")
                .ToPropertyEx(this, x => x.ErrorMessage);

            _login.ThrownExceptions
                .Select(exception => true)
                .Merge(_login.Select(unit => false))
                .ToPropertyEx(this, x => x.HasErrorMessage);
        }
        
        [ObservableAsProperty]
        public string ErrorMessage { get; }

        [ObservableAsProperty]
        public bool HasErrorMessage { get; }
        
        [ObservableAsProperty]
        public bool IsBusy { get; }
        
        public ICommand Login => _login;
    }
}