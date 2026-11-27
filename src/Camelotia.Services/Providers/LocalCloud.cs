// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using ReactiveUI.Primitives.Signals;

namespace Camelotia.Services.Providers;

/// <summary>Browses the local file system. The empty path lists the drives.</summary>
/// <remarks>File system calls block, so each one runs on the thread pool.</remarks>
[System.Diagnostics.DebuggerDisplay("LocalCloud: {Parameters}")]
public sealed class LocalCloud : ICloud
{
    /// <summary>Initializes a new instance of the <see cref="LocalCloud"/> class.</summary>
    /// <param name="model">The saved parameters.</param>
    public LocalCloud(CloudParameters model) => Parameters = model;

    /// <inheritdoc/>
    public CloudParameters Parameters { get; }

    /// <inheritdoc/>
    public Guid Id => Parameters.Id;

    /// <inheritdoc/>
    public string Name => Parameters.Type.ToString();

    /// <inheritdoc/>
    public DateTimeOffset Created => Parameters.Created;

    /// <inheritdoc/>
    public long? Size
    {
        get
        {
            var total = 0L;
            foreach (var drive in GetAllDrives())
            {
                total += drive.AvailableFreeSpace;
            }

            return total;
        }
    }

    /// <inheritdoc/>
    public IObservable<bool> IsAuthorized { get; } = Signal.Emit(true);

    /// <inheritdoc/>
    public bool SupportsDirectAuth => false;

    /// <inheritdoc/>
    public bool SupportsHostAuth => false;

    /// <inheritdoc/>
    public bool SupportsOAuth => false;

    /// <inheritdoc/>
    public bool CanCreateFolder => true;

    /// <inheritdoc/>
    public string InitialPath => string.Empty;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask OAuth(CancellationToken cancellationToken) => ValueTask.CompletedTask;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask Logout(CancellationToken cancellationToken) => ValueTask.CompletedTask;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask HostAuth(string address, int port, string login, string password, CancellationToken cancellationToken) => ValueTask.CompletedTask;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask DirectAuth(string login, string password, CancellationToken cancellationToken) => ValueTask.CompletedTask;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<IEnumerable<FileModel>> GetFiles(string path, CancellationToken cancellationToken) =>
        new(Task.Run<IEnumerable<FileModel>>(() => ListFiles(path, cancellationToken), cancellationToken));

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<IEnumerable<FolderModel>> GetBreadCrumbs(string path, CancellationToken cancellationToken) =>
        new(Task.Run<IEnumerable<FolderModel>>(() => ListBreadCrumbs(path, cancellationToken), cancellationToken));

    /// <inheritdoc/>
    public async ValueTask DownloadFile(string fromPath, Stream fromStream, CancellationToken cancellationToken)
    {
        if (IsDirectory(fromPath))
        {
            throw new InvalidOperationException("A folder cannot be downloaded.");
        }

        var fileStream = File.OpenRead(fromPath);
        await using (fileStream.ConfigureAwait(false))
        {
            await fileStream.CopyToAsync(fromStream, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask CreateFolder(string path, string name, CancellationToken cancellationToken) =>
        new(Task.Run(() => CreateDirectory(path, name), cancellationToken));

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask RenameFile(string path, string name, CancellationToken cancellationToken) =>
        new(Task.Run(() => Rename(path, name), cancellationToken));

    /// <inheritdoc/>
    public async ValueTask UploadFile(string toPath, Stream fromStream, string name, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(fromStream);
        if (!IsDirectory(toPath))
        {
            throw new InvalidOperationException("A file can only be uploaded into a folder.");
        }

        var fileStream = File.Create(Path.Combine(toPath, name));
        await using (fileStream.ConfigureAwait(false))
        {
            await fromStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask Delete(string path, bool isFolder, CancellationToken cancellationToken) =>
        new(Task.Run(() => DeleteEntry(path), cancellationToken));

    /// <summary>Checks whether a drive should be listed: it is ready and is not an optical drive.</summary>
    /// <param name="type">The drive type.</param>
    /// <param name="isReady">Whether the drive is ready to read.</param>
    /// <returns><see langword="true"/> when the drive should be listed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsBrowsableDrive(DriveType type, bool isReady) => type != DriveType.CDRom && isReady;

    /// <summary>Lists the drives for the empty path, or the entries of a folder.</summary>
    /// <param name="path">The folder, or empty for the drives.</param>
    /// <param name="cancellationToken">Stops the listing between entries.</param>
    /// <returns>The entries.</returns>
    /// <exception cref="DirectoryNotFoundException">The folder does not exist.</exception>
    private static List<FileModel> ListFiles(string path, CancellationToken cancellationToken)
    {
        var files = new List<FileModel>();
        if (string.IsNullOrWhiteSpace(path))
        {
            foreach (var drive in GetAllDrives())
            {
                files.Add(new() { Name = drive.Name, IsFolder = true, Size = drive.AvailableFreeSpace, Path = drive.Name });
            }

            return files;
        }

        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"The folder '{path}' does not exist.");
        }

        foreach (var entry in new DirectoryInfo(path).EnumerateFileSystemInfos())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var isFolder = (entry.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
            var size = entry is FileInfo file ? file.Length : 0;
            files.Add(new() { Path = entry.FullName, Name = entry.Name, IsFolder = isFolder, Modified = entry.LastWriteTimeUtc, Size = size });
        }

        return files;
    }

    /// <summary>Lists the folders from the drive root down to a folder, with the subfolders of each.</summary>
    /// <param name="path">The folder the trail ends at.</param>
    /// <param name="cancellationToken">Stops the listing between folders.</param>
    /// <returns>The trail, root first, or an empty trail when the folder does not exist.</returns>
    private static List<FolderModel> ListBreadCrumbs(string path, CancellationToken cancellationToken)
    {
        var trail = new List<FolderModel>();
        if (!Directory.Exists(path))
        {
            return trail;
        }

        for (var folder = new DirectoryInfo(path); folder is not null; folder = folder.Parent)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var children = new List<FolderModel>();
            foreach (var child in folder.EnumerateDirectories())
            {
                children.Add(new(child.FullName, child.Name));
            }

            trail.Add(new(folder.FullName, folder.Name, children));
        }

        trail.Reverse();
        return trail;
    }

    /// <summary>Creates a folder inside another folder.</summary>
    /// <param name="path">The parent folder.</param>
    /// <param name="name">The new folder name.</param>
    /// <exception cref="InvalidOperationException">The parent is a file.</exception>
    private static void CreateDirectory(string path, string name)
    {
        if (!IsDirectory(path))
        {
            throw new InvalidOperationException("A folder can only be created inside another folder.");
        }

        _ = Directory.CreateDirectory(Path.Combine(path, name));
    }

    /// <summary>Renames a file or folder in place.</summary>
    /// <param name="path">The entry to rename.</param>
    /// <param name="name">The new name.</param>
    /// <exception cref="InvalidOperationException">The path is a drive root.</exception>
    private static void Rename(string path, string name)
    {
        var parent = Path.GetDirectoryName(path) ?? throw new InvalidOperationException("A drive cannot be renamed.");
        var target = Path.Combine(parent, name);
        if (IsDirectory(path))
        {
            Directory.Move(path, target);
        }
        else
        {
            File.Move(path, target);
        }
    }

    /// <summary>Deletes a file or an empty folder.</summary>
    /// <param name="path">The entry to delete.</param>
    private static void DeleteEntry(string path)
    {
        if (IsDirectory(path))
        {
            Directory.Delete(path, recursive: false);
        }
        else
        {
            File.Delete(path);
        }
    }

    /// <summary>Lists the ready drives, leaving out optical drives.</summary>
    /// <returns>The drives.</returns>
    private static List<DriveInfo> GetAllDrives()
    {
        var drives = new List<DriveInfo>();
        foreach (var drive in DriveInfo.GetDrives())
        {
            if (IsBrowsableDrive(drive.DriveType, drive.IsReady))
            {
                drives.Add(drive);
            }
        }

        return drives;
    }

    /// <summary>Checks whether a path is a folder.</summary>
    /// <param name="path">The path to check.</param>
    /// <returns><see langword="true"/> when the path is a folder.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsDirectory(string path) => (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
}
