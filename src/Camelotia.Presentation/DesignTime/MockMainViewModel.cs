using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Models;
using ReactiveUI;

namespace Camelotia.Presentation.DesignTime
{
    public class MockMainViewModel : ReactiveObject, IMainViewModel
    {
        public ReadOnlyObservableCollection<IProviderViewModel> Providers { get; } = 
            new ReadOnlyObservableCollection<IProviderViewModel>(
                new ObservableCollection<IProviderViewModel>(
                    new List<IProviderViewModel>
                    {
                        new MockProviderViewModel(),
                        new MockProviderViewModel()
                    }));
        
        public IProviderViewModel SelectedProvider { get; set; } = new MockProviderViewModel();

        public IEnumerable<ProviderType> SupportedTypes { get; } = new[] {ProviderType.Ftp, ProviderType.Sftp};

        public ProviderType SelectedSupportedType { get; set; } = ProviderType.Sftp;

        public bool WelcomeScreenCollapsed { get; } = true;
        
        public bool WelcomeScreenVisible { get; }
        
        public ICommand Unselect { get; }
        
        public ICommand Refresh { get; }
        
        public ICommand Remove { get; }
        
        public ICommand Add { get; }
        
        public bool IsLoading { get; }
        
        public bool IsReady { get; }
    }
}