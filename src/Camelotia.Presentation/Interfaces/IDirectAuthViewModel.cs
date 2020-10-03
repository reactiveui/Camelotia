using System.ComponentModel;
using System.Reactive;
using ReactiveUI;

namespace Camelotia.Presentation.Interfaces
{
    public interface IDirectAuthViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        string Username { get; set; }
        
        string Password { get; set; }
        
        ReactiveCommand<Unit, Unit> Login { get; }
        
        bool HasErrorMessage { get; }
        
        string ErrorMessage { get; }
        
        bool IsBusy { get; }
    }
}