using System.Collections.Generic;
using System.Windows.Input;
using Camelotia.Services.Models;
using ReactiveUI;

namespace Camelotia.Presentation.Interfaces
{
    public interface IProviderViewModel : IReactiveObject, ISupportsActivation
    {
        IAuthViewModel Auth { get; }
        
        FileModel SelectedFile { get; set; }
        
        IEnumerable<FileModel> Files { get; }
        
        ICommand DownloadSelectedFile { get; }
        
        ICommand UploadToCurrentPath { get; }
        
        ICommand Refresh { get; }
        
        ICommand Logout { get; }
        
        ICommand Back { get; }
        
        ICommand Open { get; }
        
        bool IsCurrentPathEmpty { get; }
        
        bool IsLoading { get; }
        
        bool IsReady { get; }
        
        bool HasErrors { get; }
        
        bool CanLogout { get; }
        
        string CurrentPath { get; }
        
        string Description { get; }
        
        string Name { get; }
        
        string Size { get; }
    }
}