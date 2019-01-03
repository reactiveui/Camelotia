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
    public sealed class LocalFileSystemProvider : IProvider
    {
        public string Size => GetSizeOnAllDisks();

        public string Name => "Local File System";

        public string Description => "Provides access to files stored locally.";

        public IObservable<bool> IsAuthorized { get; } = Observable.Return(true);

        public bool SupportsDirectAuth => false;

        public bool SupportsHostAuth => false;

        public bool SupportsOAuth => false;

        public string InitialPath => string.Empty;

        public Task OAuth() => Task.CompletedTask;

        public Task Logout() => Task.CompletedTask;

        public Task HostAuth(string address, string login, string password) => Task.CompletedTask;

        public Task DirectAuth(string login, string password) => Task.CompletedTask;

        public Task<IEnumerable<FileModel>> Get(string path) => Task.Run(() =>
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                var driveQuery = from entity in GetAllDrives()
                                 where entity.IsReady
                                 let size = ByteConverter.BytesToString(entity.AvailableFreeSpace)
                                 select new FileModel(entity.Name, entity.Name, true, size);
                return driveQuery
                    .ToList()
                    .AsEnumerable();
            }

            if (!Directory.Exists(path))
                throw new ArgumentException("Directory doesn't exist.");

            var query = from entity in Directory.GetFileSystemEntries(path)
                        let isDirectory = IsDirectory(entity)
                        let size = isDirectory ? "*" : ByteConverter.BytesToString(new FileInfo(entity).Length)
                        select new FileModel(Path.GetFileName(entity), entity, isDirectory, size);

            return query
                .ToList()
                .AsEnumerable();
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

        public Task Delete(FileModel file) => Task.Run(() =>
        {
            var isDirectory = IsDirectory(file.Path);
            if (isDirectory) Directory.Delete(file.Path, false);
            else File.Delete(file.Path);
        });

        private static string GetSizeOnAllDisks()
        {
            var totalBytes = GetAllDrives()
                .Select(x => x.AvailableFreeSpace)
                .Sum();
            
            return ByteConverter.BytesToString(totalBytes);
        }

        private static IEnumerable<DriveInfo> GetAllDrives()
        {
            var drives = DriveInfo
                .GetDrives()
                .Where(p => p.DriveType != DriveType.CDRom);

            return drives;
        }

        private static bool IsDirectory(string path)
        {
            var attributes = File.GetAttributes(path);
            return attributes.HasFlag(FileAttributes.Directory);
        }
    }
}