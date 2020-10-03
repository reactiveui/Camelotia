using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Models;
using ReactiveUI;

namespace Camelotia.Presentation.DesignTime
{
    public class DesignTimeMainViewModel : ReactiveObject, IMainViewModel
    {
        public ReadOnlyObservableCollection<IProviderViewModel> Providers { get; } = 
            new ReadOnlyObservableCollection<IProviderViewModel>(
                new ObservableCollection<IProviderViewModel>(
                    new List<IProviderViewModel>
                    {
                        new DesignTimeProviderViewModel(),
                        new DesignTimeProviderViewModel()
                    }));
        
        public IProviderViewModel SelectedProvider { get; set; } = new DesignTimeProviderViewModel();

        public IEnumerable<ProviderType> SupportedTypes { get; } = new[] {ProviderType.Ftp, ProviderType.Sftp};

        public ProviderType SelectedSupportedType { get; set; } = ProviderType.Sftp;

        public bool WelcomeScreenCollapsed { get; } = true;
        
        public bool WelcomeScreenVisible { get; }
        
        public ReactiveCommand<Unit, Unit> Unselect { get; }
        
        public ReactiveCommand<Unit, Unit> Refresh { get; }
        
        public ReactiveCommand<Unit, Unit> Remove { get; }
        
        public ReactiveCommand<Unit, Unit> Add { get; }
        
        public bool IsLoading { get; }

        public bool IsReady { get; } = true;
    }
}