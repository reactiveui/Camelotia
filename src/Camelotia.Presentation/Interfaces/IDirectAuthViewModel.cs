using System.ComponentModel;
using System.Reactive;
using ReactiveUI;
using ReactiveUI.Validation.Abstractions;

namespace Camelotia.Presentation.Interfaces
{
    public interface IDirectAuthViewModel :
        INotifyPropertyChanged,
        INotifyDataErrorInfo,
        IValidatableViewModel,
        IReactiveObject
    {
        string Username { get; set; }
        
        string Password { get; set; }
        
        ReactiveCommand<Unit, Unit> Login { get; }
        
        bool HasErrorMessage { get; }
        
        string ErrorMessage { get; }
        
        bool IsBusy { get; }
    }
}