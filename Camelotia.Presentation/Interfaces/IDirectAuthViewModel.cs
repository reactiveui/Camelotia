using System.Windows.Input;
using ReactiveUI;

namespace Camelotia.Presentation.Interfaces
{
    public interface IDirectAuthViewModel : IReactiveObject, ISupportsActivation
    {
        string Username { get; set; }
        
        string Password { get; set; }
        
        ICommand Login { get; }
        
        bool HasErrors { get; }
        
        string ErrorMessage { get; }
        
        bool IsBusy { get; }
    }
}