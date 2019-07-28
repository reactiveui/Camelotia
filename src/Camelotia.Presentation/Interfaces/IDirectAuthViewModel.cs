using System.ComponentModel;
using System.Windows.Input;

namespace Camelotia.Presentation.Interfaces
{
    public interface IDirectAuthViewModel : INotifyPropertyChanged
    {
        string Username { get; set; }
        
        string Password { get; set; }
        
        ICommand Login { get; }
        
        bool HasErrors { get; }
        
        string ErrorMessage { get; }
        
        bool IsBusy { get; }
    }
}