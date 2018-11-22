using System.Collections.Generic;
using System.Windows.Input;
using ReactiveUI;

namespace Camelotia.Presentation.Interfaces
{
    public interface IMainViewModel : IReactiveObject, ISupportsActivation
    {
        IProviderViewModel SelectedProvider { get; set; }
        
        IEnumerable<IProviderViewModel> Providers { get; }

        ICommand LoadProviders { get; }
        
        bool IsLoading { get; }
        
        bool IsReady { get; }
    }
}