// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Services.Models;

namespace Camelotia.Services.Interfaces;

/// <summary>A file storage provider that Camelotia can browse.</summary>
public interface ICloud
{
    /// <summary>Gets the unique identifier of this provider instance.</summary>
    Guid Id { get; }

    /// <summary>Gets the total size of the storage in bytes, or <see langword="null"/> when the provider does not report it.</summary>
    long? Size { get; }

    /// <summary>Gets the display name of the provider.</summary>
    string Name { get; }

    /// <summary>Gets the time the user added this provider.</summary>
    DateTimeOffset Created { get; }

    /// <summary>Gets the path the browser opens first.</summary>
    string InitialPath { get; }

    /// <summary>Gets a value indicating whether the provider can create folders.</summary>
    bool CanCreateFolder { get; }

    /// <summary>Gets a signal that reports whether the user is signed in.</summary>
    IObservable<bool> IsAuthorized { get; }

    /// <summary>Gets the saved parameters, such as the token, for this provider.</summary>
    CloudParameters Parameters { get; }

    /// <summary>Gets a value indicating whether the provider signs in with a user name and password.</summary>
    bool SupportsDirectAuth { get; }

    /// <summary>Gets a value indicating whether the provider signs in with a host, port, user name and password.</summary>
    bool SupportsHostAuth { get; }

    /// <summary>Gets a value indicating whether the provider signs in through a browser.</summary>
    bool SupportsOAuth { get; }

    /// <summary>Lists the files and folders at a path.</summary>
    /// <param name="path">The folder to list.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The entries in the folder.</returns>
    ValueTask<IEnumerable<FileModel>> GetFiles(string path, CancellationToken cancellationToken);

    /// <summary>Lists the folders from the root down to a path, with the subfolders of each.</summary>
    /// <param name="path">The folder the trail ends at.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The folders on the trail.</returns>
    ValueTask<IEnumerable<FolderModel>> GetBreadCrumbs(string path, CancellationToken cancellationToken);

    /// <summary>Uploads a file into a folder.</summary>
    /// <param name="toPath">The folder to upload into.</param>
    /// <param name="fromStream">The file content.</param>
    /// <param name="name">The file name.</param>
    /// <param name="cancellationToken">Cancels the upload.</param>
    /// <returns>A task that completes when the upload finishes.</returns>
    ValueTask UploadFile(string toPath, Stream fromStream, string name, CancellationToken cancellationToken);

    /// <summary>Downloads a file into a stream.</summary>
    /// <param name="fromPath">The file to download.</param>
    /// <param name="fromStream">The stream that receives the content.</param>
    /// <param name="cancellationToken">Cancels the download.</param>
    /// <returns>A task that completes when the download finishes.</returns>
    ValueTask DownloadFile(string fromPath, Stream fromStream, CancellationToken cancellationToken);

    /// <summary>Creates a folder.</summary>
    /// <param name="path">The parent folder.</param>
    /// <param name="name">The new folder name.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>A task that completes when the folder exists.</returns>
    ValueTask CreateFolder(string path, string name, CancellationToken cancellationToken);

    /// <summary>Renames a file or folder.</summary>
    /// <param name="path">The entry to rename.</param>
    /// <param name="name">The new name.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>A task that completes when the rename finishes.</returns>
    ValueTask RenameFile(string path, string name, CancellationToken cancellationToken);

    /// <summary>Deletes a file or folder.</summary>
    /// <param name="path">The entry to delete.</param>
    /// <param name="isFolder">Whether the entry is a folder.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>A task that completes when the entry is gone.</returns>
    ValueTask Delete(string path, bool isFolder, CancellationToken cancellationToken);

    /// <summary>Signs in with a user name and password.</summary>
    /// <param name="login">The user name.</param>
    /// <param name="password">The password.</param>
    /// <param name="cancellationToken">Cancels the sign-in.</param>
    /// <returns>A task that completes when the user is signed in.</returns>
    ValueTask DirectAuth(string login, string password, CancellationToken cancellationToken);

    /// <summary>Signs in to a server.</summary>
    /// <param name="address">The server address.</param>
    /// <param name="port">The server port.</param>
    /// <param name="login">The user name.</param>
    /// <param name="password">The password.</param>
    /// <param name="cancellationToken">Cancels the sign-in.</param>
    /// <returns>A task that completes when the user is signed in.</returns>
    ValueTask HostAuth(string address, int port, string login, string password, CancellationToken cancellationToken);

    /// <summary>Signs in through a browser.</summary>
    /// <param name="cancellationToken">Cancels the sign-in.</param>
    /// <returns>A task that completes when the user is signed in.</returns>
    ValueTask OAuth(CancellationToken cancellationToken);

    /// <summary>Signs out and forgets the saved token.</summary>
    /// <param name="cancellationToken">Cancels the sign-out.</param>
    /// <returns>A task that completes when the user is signed out.</returns>
    ValueTask Logout(CancellationToken cancellationToken);
}
