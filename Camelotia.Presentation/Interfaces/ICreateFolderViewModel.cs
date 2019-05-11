using System.ComponentModel;
using System.Windows.Input;

namespace Camelotia.Presentation.Interfaces
{
    public delegate ICreateFolderViewModel CreateFolderViewModelFactory(IProviderViewModel providerViewModel);

    public interface ICreateFolderViewModel : INotifyPropertyChanged
    {
        bool IsLoading { get; }
        
        bool IsVisible { get; set; }
        
        bool HasErrors { get; }
        
        string Name { get; set; }
        
        string Path { get; }
        
        string ErrorMessage { get; }
        
        ICommand Create { get; }
        
        ICommand Close { get; }
        
        ICommand Open { get; }
    }
}