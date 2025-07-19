using System.ComponentModel;
using System.Reactive;
using Camelotia.Services.Models;
using ReactiveUI;

namespace Camelotia.Presentation.Interfaces;

public interface ICloudViewModel : INotifyPropertyChanged
{
    Guid Id { get; }

    IAuthViewModel Auth { get; }

    IRenameFileViewModel Rename { get; }

    ICreateFolderViewModel Folder { get; }

    IFileViewModel SelectedFile { get; set; }

    IEnumerable<IFileViewModel> Files { get; }

    IEnumerable<IFolderViewModel> BreadCrumbs { get; }

    ReactiveCommand<Unit, Unit> DownloadSelectedFile { get; }

    ReactiveCommand<Unit, Unit> UploadToCurrentPath { get; }

    ReactiveCommand<Unit, Unit> DeleteSelectedFile { get; }

    ReactiveCommand<Unit, Unit> UnselectFile { get; }

    ReactiveCommand<Unit, IEnumerable<FileModel>> Refresh { get; }

    ReactiveCommand<Unit, Unit> Logout { get; }

    ReactiveCommand<Unit, string> Back { get; }

    ReactiveCommand<Unit, string> Open { get; }

    ReactiveCommand<string, string> SetPath { get; }

    bool IsCurrentPathEmpty { get; }

    bool IsLoading { get; }

    bool IsReady { get; }

    bool HasErrorMessage { get; }

    bool CanLogout { get; }

    bool CanInteract { get; }

    bool ShowBreadCrumbs { get; }

    bool HideBreadCrumbs { get; }

    int RefreshingIn { get; }

    string CurrentPath { get; }

    string Description { get; }

    DateTime Created { get; }

    string Name { get; }

    string Size { get; }
}
