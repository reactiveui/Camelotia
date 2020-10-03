using System.Reactive;
using System.Reactive.Linq;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Camelotia.Presentation.ViewModels
{
    public sealed class OAuthViewModel : ReactiveObject, IOAuthViewModel
    {
        public OAuthViewModel(IProvider provider)
        {
            Login = ReactiveCommand.CreateFromTask(provider.OAuth);
            Login.IsExecuting.ToPropertyEx(this, x => x.IsBusy);

            Login.ThrownExceptions
                .Select(exception => exception.Message)
                .Log(this, $"OAuth error occured in {provider.Name}")
                .ToPropertyEx(this, x => x.ErrorMessage);

            Login.ThrownExceptions
                .Select(exception => true)
                .Merge(Login.Select(unit => false))
                .ToPropertyEx(this, x => x.HasErrorMessage);
        }
        
        [ObservableAsProperty]
        public string ErrorMessage { get; }

        [ObservableAsProperty]
        public bool HasErrorMessage { get; }
        
        [ObservableAsProperty]
        public bool IsBusy { get; }
        
        public ReactiveCommand<Unit, Unit> Login { get; }
    }
}