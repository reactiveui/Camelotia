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
        
        public bool SupportsOAuth => false;

        public Task OAuth() => Task.CompletedTask;

        public Task Logout() => Task.CompletedTask;

        public Task DirectAuth(string login, string password) => Task.CompletedTask;

        public Task<IEnumerable<FileModel>> Get(string path) => Task.Run(() =>
        {
            if (!Directory.Exists(path))
                throw new ArgumentException("Directory doesn't exist.");
            
            var query = from entity in Directory.GetFileSystemEntries(path) 
                        let isDirectory = IsDirectory(entity) 
                        let size = isDirectory ? "*" : BytesToString(new FileInfo(entity).Length) 
                        select new FileModel(Path.GetFileName(entity), entity, isDirectory, size);

            return query
                .ToList()
                .AsEnumerable();
        });

        public Task DownloadFile(string from, Stream to)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));
            if (IsDirectory(from)) throw new InvalidOperationException("Can't download directory.");

            using (var fileStream = File.OpenRead(from))
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                fileStream.CopyTo(to);
            }

            return Task.CompletedTask;
        }

        public Task UploadFile(string to, Stream from)
        {
            if (to == null) throw new ArgumentNullException(nameof(to));
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (!IsDirectory(to)) throw new InvalidOperationException("Can't upload to a non-directory.");

            var path = Path.Combine(to, DateTime.Now.Ticks.ToString());
            using (var fileStream = File.Create(path))
            {
                from.Seek(0, SeekOrigin.Begin);
                from.CopyTo(fileStream);
            }

            return Task.CompletedTask;
        }

        private static string GetSizeOnAllDisks()
        {
            var totalBytes = DriveInfo
                .GetDrives()
                .Select(x => x.AvailableFreeSpace)
                .Sum();
            
            return BytesToString(totalBytes);
        }
        
        private static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            
            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num) + suf[place];
        }

        private static bool IsDirectory(string path)
        {
            var attributes = File.GetAttributes(path);
            return attributes.HasFlag(FileAttributes.Directory);
        }
    }
}