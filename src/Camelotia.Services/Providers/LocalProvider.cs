using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;

namespace Camelotia.Services.Providers
{
    public sealed class LocalProvider : IProvider
    {
        private readonly ProviderParameters _model;

        public LocalProvider(ProviderParameters model) => _model = model;

        public Guid Id => _model.Id;

        public string Name => _model.Type.ToString();

        public DateTime Created => _model.Created;

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

        public Task<IEnumerable<FileModel>> Get(string path) => Task.Run(() =>
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return GetAllDrives().Select(file => new FileModel
                {
                    Name = file.Name,
                    IsFolder = true,
                    Size = file.AvailableFreeSpace,
                    Path = file.Name
                });
            }

            if (!Directory.Exists(path))
                throw new ArgumentException("Directory doesn't exist.");

            return from file in Directory.GetFileSystemEntries(path)
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
        });

        public async Task DownloadFile(string from, Stream to)
        {
            if (IsDirectory(from)) throw new InvalidOperationException("Can't download directory.");

            using (var fileStream = File.OpenRead(from))
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                await fileStream.CopyToAsync(to);
            }
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
            using (var fileStream = File.Create(path))
            {
                from.Seek(0, SeekOrigin.Begin);
                await from.CopyToAsync(fileStream);
            }
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

        private static IEnumerable<DriveInfo> GetAllDrives()
        {
            var drives = DriveInfo
                .GetDrives()
                .Where(p => p.DriveType != DriveType.CDRom && p.IsReady);

            return drives;
        }

        private static bool IsDirectory(string path)
        {
            var attributes = File.GetAttributes(path);
            return attributes.HasFlag(FileAttributes.Directory);
        }
    }
}