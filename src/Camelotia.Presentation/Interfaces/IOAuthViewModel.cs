using System.ComponentModel;
using System.Windows.Input;

namespace Camelotia.Presentation.Interfaces
{
    public interface IOAuthViewModel : INotifyPropertyChanged
    {
        ICommand Login { get; }
        
        bool HasErrors { get; }
        
        string ErrorMessage { get; }
        
        bool IsBusy { get; }
    }
}