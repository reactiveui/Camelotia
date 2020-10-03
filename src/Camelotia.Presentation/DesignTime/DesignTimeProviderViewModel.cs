using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Models;
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

        public ReactiveCommand<Unit, Unit> DownloadSelectedFile { get; }
        
        public ReactiveCommand<Unit, Unit> UploadToCurrentPath { get; }
        
        public ReactiveCommand<Unit, Unit> DeleteSelectedFile { get; }
        
        public ReactiveCommand<Unit, Unit> UnselectFile { get; }
        
        public ReactiveCommand<Unit, IEnumerable<FileModel>> Refresh { get; }
        
        public ReactiveCommand<Unit, Unit> Logout { get; }
        
        public ReactiveCommand<Unit, string> Back { get; }
        
        public ReactiveCommand<Unit, string> Open { get; }

        public ReactiveCommand<string, string> SetPath { get; }

        public bool IsCurrentPathEmpty { get; }
        
        public bool IsLoading { get; }

        public bool IsReady { get; } = true;

        public bool HasErrorMessage { get; }
        
        public bool CanLogout { get; }
        
        public bool CanInteract { get; }

        public bool ShowBreadCrumbs { get; } = true;

        public int RefreshingIn { get; } = 30;

        public string CurrentPath { get; } = "/home/files";

        public IEnumerable<IFolderViewModel> BreadCrumbs { get; } = new List<IFolderViewModel>
        {
            new DesignTimeFolderViewModel 
            (
                name: "home", 
                children: new[] 
                { 
                    new DesignTimeFolderViewModel(name: "home"),
                    new DesignTimeFolderViewModel(name: "home1"), 
                    new DesignTimeFolderViewModel(name: "home2") 
                }
            ),
            new DesignTimeFolderViewModel
            (
                name: "files",
                children: new[]
                {
                    new DesignTimeFolderViewModel(name: "files"),
                    new DesignTimeFolderViewModel(name: "files1"),
                    new DesignTimeFolderViewModel(name: "files2")
                }
            )
        };

        public string Description { get; } = "Mock file system.";
        
        public DateTime Created { get; } = DateTime.Now;

        public string Name { get; } = "Awesome mock";

        public string Size { get; } = "42MB";
    }
}