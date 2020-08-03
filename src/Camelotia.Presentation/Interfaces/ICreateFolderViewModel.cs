using System.ComponentModel;
using System.Windows.Input;

namespace Camelotia.Presentation.Interfaces
{
    public interface ICreateFolderViewModel : INotifyPropertyChanged
    {
        bool IsLoading { get; }
        
        bool IsVisible { get; set; }
        
        string Name { get; set; }
        
        string Path { get; }
        
        bool HasErrorMessage { get; }
        
        string ErrorMessage { get; }
        
        ICommand Create { get; }
        
        ICommand Close { get; }
        
        ICommand Open { get; }
    }
}