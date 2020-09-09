using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace Camelotia.Presentation.Interfaces
{
    public interface IProviderViewModel : INotifyPropertyChanged
    {
        Guid Id { get; }
        
        IAuthViewModel Auth { get; }

        IRenameFileViewModel Rename { get; }

        ICreateFolderViewModel Folder { get; }

        IFileViewModel SelectedFile { get; set; }
        
        IEnumerable<IFileViewModel> Files { get; }
        
        ICommand DownloadSelectedFile { get; }
        
        ICommand UploadToCurrentPath { get; }

        ICommand DeleteSelectedFile { get; }
        
        ICommand UnselectFile { get; }

        ICommand Refresh { get; }
        
        ICommand Logout { get; }
        
        ICommand Back { get; }
        
        ICommand Open { get; }

        ICommand SetPath { get; }

        bool IsCurrentPathEmpty { get; }
        
        bool IsLoading { get; }
        
        bool IsReady { get; }
        
        bool HasErrorMessage { get; }
        
        bool CanLogout { get; }
        
        bool CanInteract { get; }
        
        int RefreshingIn { get; }
        
        string CurrentPath { get; }
        
        string Description { get; }

        DateTime Created { get; }
        
        string Name { get; }
        
        string Size { get; }
    }
}