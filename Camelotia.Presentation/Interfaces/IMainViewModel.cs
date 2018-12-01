using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace Camelotia.Presentation.Interfaces
{
    public interface IMainViewModel : INotifyPropertyChanged
    {
        IProviderViewModel SelectedProvider { get; set; }
        
        IEnumerable<IProviderViewModel> Providers { get; }

        ICommand LoadProviders { get; }
        
        bool IsLoading { get; }
        
        bool IsReady { get; }
    }
}