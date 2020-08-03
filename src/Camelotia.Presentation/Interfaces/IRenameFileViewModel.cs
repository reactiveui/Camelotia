using System.ComponentModel;
using System.Windows.Input;

namespace Camelotia.Presentation.Interfaces
{
    public interface IRenameFileViewModel : INotifyPropertyChanged
    {
        bool IsLoading { get; }
        
        bool IsVisible { get; set; }
        
        string OldName { get; }
        
        string NewName { get; set; }
        
        bool HasErrorMessage { get; }
        
        string ErrorMessage { get; }
        
        ICommand Rename { get; }
        
        ICommand Close { get; }
        
        ICommand Open { get; }
    }
}