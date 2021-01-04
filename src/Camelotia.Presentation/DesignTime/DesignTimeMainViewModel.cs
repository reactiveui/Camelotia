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
        public ReadOnlyObservableCollection<ICloudViewModel> Clouds { get; } =
            new ReadOnlyObservableCollection<ICloudViewModel>(
                new ObservableCollection<ICloudViewModel>(
                    new List<ICloudViewModel>
                    {
                        new DesignTimeCloudViewModel(),
                        new DesignTimeCloudViewModel()
                    }));

        public ICloudViewModel SelectedProvider { get; set; } = new DesignTimeCloudViewModel();

        public IEnumerable<CloudType> SupportedTypes { get; } = new[] { CloudType.Ftp, CloudType.Sftp };

        public CloudType SelectedSupportedType { get; set; } = CloudType.Sftp;

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