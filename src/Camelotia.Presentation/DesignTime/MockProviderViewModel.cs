using System;
using System.Collections.Generic;
using System.Windows.Input;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.DesignTime
{
    public class MockProviderViewModel : ReactiveObject, IProviderViewModel
    {
        public Guid Id { get; } = Guid.NewGuid();
        
        public IAuthViewModel Auth { get; } = new MockAuthViewModel();
        
        public IRenameFileViewModel Rename { get; } = new MockRenameFileViewModel();
        
        public ICreateFolderViewModel Folder { get; } = new MockCreateFolderViewModel();
        
        public IFileViewModel SelectedFile { get; set; } = new MockFileViewModel();

        public IEnumerable<IFileViewModel> Files { get; } = new[] {new MockFileViewModel(), new MockFileViewModel()};
        
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