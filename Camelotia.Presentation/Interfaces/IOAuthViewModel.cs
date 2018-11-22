using System.Windows.Input;
using ReactiveUI;

namespace Camelotia.Presentation.Interfaces
{
    public interface IOAuthViewModel : IReactiveObject, ISupportsActivation
    {
        ICommand Login { get; }
        
        bool HasErrors { get; }
        
        string ErrorMessage { get; }
        
        bool IsBusy { get; }
    }
}