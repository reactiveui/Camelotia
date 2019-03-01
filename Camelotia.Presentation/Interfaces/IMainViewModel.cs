using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Camelotia.Presentation.Interfaces
{
    public interface IMainViewModel : INotifyPropertyChanged
    {
        ReadOnlyObservableCollection<IProviderViewModel> Providers { get; }
        
        IProviderViewModel SelectedProvider { get; set; }
        
        IEnumerable<string> SupportedTypes { get; }

        string SelectedSupportedType { get; set; }

        ICommand Refresh { get; }

        ICommand Remove { get; }

        ICommand Add { get; }
        
        bool IsLoading { get; }
        
        bool IsReady { get; }
    }
}