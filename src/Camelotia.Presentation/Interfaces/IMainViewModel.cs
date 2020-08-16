using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Camelotia.Services.Models;

namespace Camelotia.Presentation.Interfaces
{
    public interface IMainViewModel : INotifyPropertyChanged
    {
        ReadOnlyObservableCollection<IProviderViewModel> Providers { get; }
        
        IProviderViewModel SelectedProvider { get; set; }
        
        IEnumerable<ProviderType> SupportedTypes { get; }

        ProviderType SelectedSupportedType { get; set; }

        bool WelcomeScreenCollapsed { get; }

        bool WelcomeScreenVisible { get; }

        ICommand Unselect { get; }

        ICommand Refresh { get; }

        ICommand Remove { get; }

        ICommand Add { get; }
        
        bool IsLoading { get; }
        
        bool IsReady { get; }
    }
}