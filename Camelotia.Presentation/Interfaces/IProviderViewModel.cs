using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Camelotia.Services.Models;

namespace Camelotia.Presentation.Interfaces
{
    public interface IProviderViewModel : INotifyPropertyChanged
    {
        IAuthViewModel Auth { get; }
        
        ICreateFolderViewModel Folder { get; }
        
        IRenameFileViewModel Rename { get; }
        
        FileModel SelectedFile { get; set; }
        
        IEnumerable<FileModel> Files { get; }
        
        ICommand DownloadSelectedFile { get; }
        
        ICommand UploadToCurrentPath { get; }

        ICommand DeleteSelectedFile { get; }
        
        ICommand UnselectFile { get; }

        ICommand Refresh { get; }
        
        ICommand Logout { get; }
        
        ICommand Back { get; }
        
        ICommand Open { get; }
        
        bool IsCurrentPathEmpty { get; }
        
        bool IsLoading { get; }
        
        bool IsReady { get; }
        
        bool HasErrors { get; }
        
        bool CanLogout { get; }
        
        bool CanInteract { get; }
        
        int RefreshingIn { get; }
        
        string CurrentPath { get; }
        
        string Description { get; }
        
        string Name { get; }
        
        string Size { get; }
    }
}