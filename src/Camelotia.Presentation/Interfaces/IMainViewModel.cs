using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using Camelotia.Services.Models;
using ReactiveUI;

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

        ReactiveCommand<Unit, Unit> Unselect { get; }

        ReactiveCommand<Unit, Unit> Refresh { get; }

        ReactiveCommand<Unit, Unit> Remove { get; }

        ReactiveCommand<Unit, Unit> Add { get; }
        
        bool IsLoading { get; }
        
        bool IsReady { get; }
    }
}