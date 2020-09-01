using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.DesignTime
{
    public class DesignTimeProviderViewModel : ReactiveObject, IProviderViewModel
    {
        public DesignTimeProviderViewModel()
        {
            Files = new[] 
            { 
                new DesignTimeFileViewModel(this),
                new DesignTimeFileViewModel(this)
            };
            SelectedFile = Files.FirstOrDefault();
        }

        public Guid Id { get; } = Guid.NewGuid();
        
        public IAuthViewModel Auth { get; } = new DesignTimeAuthViewModel();
        
        public IRenameFileViewModel Rename { get; } = new DesignTimeRenameFileViewModel();
        
        public ICreateFolderViewModel Folder { get; } = new DesignTimeCreateFolderViewModel();
        
        public IFileViewModel SelectedFile { get; set; }

        public IEnumerable<IFileViewModel> Files { get; } 
        
        public ICommand DownloadSelectedFile { get; }
        
        public ICommand UploadToCurrentPath { get; }
        
        public ICommand DeleteSelectedFile { get; }
        
        public ICommand UnselectFile { get; }
        
        public ICommand Refresh { get; }
        
        public ICommand Logout { get; }
        
        public ICommand Back { get; }
        
        public ICommand Open { get; }
        
        public bool IsCurrentPathEmpty { get; }
        
        public bool IsLoading { get; }

        public bool IsReady { get; } = true;

        public bool HasErrorMessage { get; }
        
        public bool CanLogout { get; }
        
        public bool CanInteract { get; }

        public int RefreshingIn { get; } = 30;

        public string CurrentPath { get; } = "/home/files";

        public string Description { get; } = "Mock file system.";
        
        public DateTime Created { get; } = DateTime.Now;

        public string Name { get; } = "Awesome mock";

        public string Size { get; } = "42MB";
    }
}