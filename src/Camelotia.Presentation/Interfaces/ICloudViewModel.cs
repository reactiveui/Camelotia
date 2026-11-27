// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.ComponentModel;
using Camelotia.Services.Models;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Camelotia.Presentation.Interfaces;

/// <summary>Browses one storage provider.</summary>
public interface ICloudViewModel : INotifyPropertyChanged
{
    /// <summary>Gets the provider instance identifier.</summary>
    Guid Id { get; }

    /// <summary>Gets the sign-in view model.</summary>
    IAuthViewModel Auth { get; }

    /// <summary>Gets the rename form.</summary>
    IRenameFileViewModel Rename { get; }

    /// <summary>Gets the create-folder form.</summary>
    ICreateFolderViewModel Folder { get; }

    /// <summary>Gets or sets the selected entry, or <see langword="null"/> when none is selected.</summary>
    IFileViewModel? SelectedFile { get; set; }

    /// <summary>Gets the entries of the current folder, folders first.</summary>
    IEnumerable<IFileViewModel> Files { get; }

    /// <summary>Gets the breadcrumb trail to the current folder.</summary>
    IEnumerable<IFolderViewModel> BreadCrumbs { get; }

    /// <summary>Gets the command that downloads the selected file.</summary>
    ReactiveCommand<RxVoid, RxVoid> DownloadSelectedFile { get; }

    /// <summary>Gets the command that uploads a local file to the current folder.</summary>
    ReactiveCommand<RxVoid, RxVoid> UploadToCurrentPath { get; }

    /// <summary>Gets the command that deletes the selected file.</summary>
    ReactiveCommand<RxVoid, RxVoid> DeleteSelectedFile { get; }

    /// <summary>Gets the command that clears the selection.</summary>
    ReactiveCommand<RxVoid, RxVoid> UnselectFile { get; }

    /// <summary>Gets the command that lists the current folder again.</summary>
    ReactiveCommand<RxVoid, IEnumerable<FileModel>> Refresh { get; }

    /// <summary>Gets the command that signs out.</summary>
    ReactiveCommand<RxVoid, RxVoid> Logout { get; }

    /// <summary>Gets the command that moves to the parent folder.</summary>
    ReactiveCommand<RxVoid, string> Back { get; }

    /// <summary>Gets the command that opens the selected folder.</summary>
    ReactiveCommand<RxVoid, string> Open { get; }

    /// <summary>Gets the command that moves to a given folder.</summary>
    ReactiveCommand<string, string> SetPath { get; }

    /// <summary>Gets a value indicating whether the current folder is empty.</summary>
    bool IsCurrentPathEmpty { get; }

    /// <summary>Gets a value indicating whether a listing is running.</summary>
    bool IsLoading { get; }

    /// <summary>Gets a value indicating whether the first listing has finished.</summary>
    bool IsReady { get; }

    /// <summary>Gets a value indicating whether the last listing failed.</summary>
    bool HasErrorMessage { get; }

    /// <summary>Gets a value indicating whether the user can sign out.</summary>
    bool CanLogout { get; }

    /// <summary>Gets a value indicating whether no form is open.</summary>
    bool CanInteract { get; }

    /// <summary>Gets a value indicating whether the breadcrumb trail has folders to show.</summary>
    bool ShowBreadCrumbs { get; }

    /// <summary>Gets a value indicating whether the breadcrumb trail is hidden.</summary>
    bool HideBreadCrumbs { get; }

    /// <summary>Gets the number of seconds until the next automatic refresh.</summary>
    int RefreshingIn { get; }

    /// <summary>Gets the current folder.</summary>
    string CurrentPath { get; }

    /// <summary>Gets a short description of the provider.</summary>
    string Description { get; }

    /// <summary>Gets the time the user added the provider.</summary>
    DateTimeOffset Created { get; }

    /// <summary>Gets the provider name.</summary>
    string Name { get; }

    /// <summary>Gets the provider size, formatted for display.</summary>
    string Size { get; }
}
