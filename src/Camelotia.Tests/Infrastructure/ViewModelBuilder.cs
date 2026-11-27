// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;

namespace Camelotia.Tests.Infrastructure;

/// <summary>Builds the browser view model with its real forms, the same way the app does.</summary>
public static class ViewModelBuilder
{
    /// <summary>Creates a browser view model over a provider.</summary>
    /// <param name="state">The saved browser state.</param>
    /// <param name="cloud">The provider.</param>
    /// <param name="files">Asks for local files.</param>
    /// <returns>The browser view model.</returns>
    public static CloudViewModel CreateCloudViewModel(CloudState state, ICloud cloud, IFileManager files)
    {
        ArgumentNullException.ThrowIfNull(state);
        var factories = new CloudViewModelFactories(
            owner => new CreateFolderViewModel(state.CreateFolderState, owner, cloud),
            owner => new RenameFileViewModel(state.RenameFileState, owner, cloud),
            static (file, owner) => new FileViewModel(owner, file),
            static (folder, owner) => new FolderViewModel(owner, folder));
        var auth = new AuthViewModel(
            new DirectAuthViewModel(state.AuthState.DirectAuthState, cloud),
            new HostAuthViewModel(state.AuthState.HostAuthState, cloud),
            new OAuthViewModel(cloud),
            cloud);
        return new(state, factories, auth, files, cloud);
    }
}
