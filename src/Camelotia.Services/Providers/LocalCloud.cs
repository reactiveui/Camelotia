using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;

namespace Camelotia.Services.Providers;

public sealed class LocalCloud : ICloud
{
    public LocalCloud(CloudParameters model) => Parameters = model;

    public CloudParameters Parameters { get; }

    public Guid Id => Parameters.Id;

    public string Name => Parameters.Type.ToString();

    public DateTime Created => Parameters.Created;

    public long? Size => GetSizeOnAllDisks();

    public IObservable<bool> IsAuthorized { get; } = Observable.Return(true);

    public bool SupportsDirectAuth => false;

    public bool SupportsHostAuth => false;

    public bool SupportsOAuth => false;

    public bool CanCreateFolder => true;

    public string InitialPath => string.Empty;

    public Task OAuth() => Task.CompletedTask;

    public Task Logout() => Task.CompletedTask;

    public Task HostAuth(string address, int port, string login, string password) => Task.CompletedTask;

    public Task DirectAuth(string login, string password) => Task.CompletedTask;

    public Task<IEnumerable<FileModel>> GetFiles(string path) => Task.Run(() =>
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            var drives = GetAllDrives()
                .Select(file => new FileModel
                {
                    Name = file.Name,
                    IsFolder = true,
                    Size = file.AvailableFreeSpace,
                    Path = file.Name
                })
                .ToList();
            return drives;
        }

        if (!Directory.Exists(path)) throw new ArgumentException("Directory doesn't exist.");

        var query =
            from file in Directory.GetFileSystemEntries(path)
            let isDirectory = IsDirectory(file)
            let fileInfo = new FileInfo(file)
            select new FileModel
            {
                Path = file,
                Name = Path.GetFileName(file),
                IsFolder = IsDirectory(file),
                Modified = fileInfo.LastWriteTime,
                Size = isDirectory ? 0 : fileInfo.Length
            };
        return query.ToList().AsEnumerable();
    });

    public Task<IEnumerable<FolderModel>> GetBreadCrumbs(string path) => Task.Run(() =>
    {
        if (!Directory.Exists(path))
        {
            return Enumerable.Empty<FolderModel>();
        }

        return SplitPath(path)
            .Select(di => new FolderModel(di.FullName, di.Name, di.GetDirectories().Select(di => new FolderModel(di.FullName, di.Name))))
            .Reverse()
            .ToList()
            .AsEnumerable();
    });

    public async Task DownloadFile(string from, Stream to)
    {
        if (IsDirectory(from)) throw new InvalidOperationException("Can't download directory.");

        using var fileStream = File.OpenRead(@from);
        fileStream.Seek(0, SeekOrigin.Begin);
        await fileStream.CopyToAsync(to).ConfigureAwait(false);
    }

    public Task CreateFolder(string at, string name) => Task.Run(() =>
    {
        if (!IsDirectory(at)) throw new InvalidOperationException("Can't create folder at a non-directory.");

        var path = Path.Combine(at, name);
        Directory.CreateDirectory(path);
    });

    public Task RenameFile(string path, string name) => Task.Run(() =>
    {
        var directoryName = Path.GetDirectoryName(path);
        var newName = Path.Combine(directoryName, name);
        if (IsDirectory(path)) Directory.Move(path, newName);
        else File.Move(path, newName);
    });

    public async Task UploadFile(string to, Stream from, string name)
    {
        if (!IsDirectory(to)) throw new InvalidOperationException("Can't upload to a non-directory.");

        var path = Path.Combine(to, name);
        using var fileStream = File.Create(path);
        from.Seek(0, SeekOrigin.Begin);
        await from.CopyToAsync(fileStream).ConfigureAwait(false);
    }

    public Task Delete(string path, bool isFolder) => Task.Run(() =>
    {
        var isDirectory = IsDirectory(path);
        if (isDirectory) Directory.Delete(path, false);
        else File.Delete(path);
    });

    private static long GetSizeOnAllDisks()
    {
        return GetAllDrives()
            .Select(x => x.AvailableFreeSpace)
            .Sum();
    }

    private static IEnumerable<DriveInfo> GetAllDrives() =>
        DriveInfo
            .GetDrives()
            .Where(p => p.DriveType != DriveType.CDRom && p.IsReady);

    private static bool IsDirectory(string path)
    {
        var attributes = File.GetAttributes(path);
        return attributes.HasFlag(FileAttributes.Directory);
    }

    private static IEnumerable<DirectoryInfo> SplitPath(string path)
    {
        var directoryInfo = new DirectoryInfo(path);
        while (directoryInfo != null)
        {
            yield return directoryInfo;
            directoryInfo = directoryInfo.Parent;
        }
    }
}