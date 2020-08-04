using System.ComponentModel;
using System.Windows.Input;

namespace Camelotia.Presentation.Interfaces
{
    public interface IDirectAuthViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        string Username { get; set; }
        
        string Password { get; set; }
        
        ICommand Login { get; }
        
        bool HasErrorMessage { get; }
        
        string ErrorMessage { get; }
        
        bool IsBusy { get; }
    }
}