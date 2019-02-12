using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Camelotia.Presentation.Interfaces
{
    public interface IMainViewModel : INotifyPropertyChanged
    {
        ReadOnlyObservableCollection<IProviderViewModel> Providers { get; }
        
        IProviderViewModel SelectedProvider { get; set; }

        ICommand LoadProviders { get; }
        
        bool IsLoading { get; }
        
        bool IsReady { get; }
    }
}