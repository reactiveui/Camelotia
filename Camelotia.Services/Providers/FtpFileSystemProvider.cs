using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using FluentFTP;
using VkNet.Model;

namespace Camelotia.Services.Providers
{
    public sealed class FtpFileSystemProvider : IProvider
    {
        private readonly ISubject<bool> _isAuthorized = new ReplaySubject<bool>();
        private Func<FtpClient> _factory;

        public FtpFileSystemProvider() => _isAuthorized.OnNext(false);
        
        public string Size => "Unknown";

        public string Name => "FTP";

        public string InitialPath => "/";

        public string Description => "FTP remote file system.";

        public IObservable<bool> IsAuthorized => _isAuthorized;

        public bool SupportsDirectAuth => false;

        public bool SupportsHostAuth => true;

        public bool SupportsOAuth => false;

        public bool CanCreateFolder => true;

        public Task OAuth() => Task.CompletedTask;

        public Task DirectAuth(string login, string password) => Task.CompletedTask;

        public async Task HostAuth(string address, int port, string login, string password)
        {
            _factory = () => new FtpClient(address, port, login, password);
            await Get("/").ConfigureAwait(false);
            _isAuthorized.OnNext(true);
        }
        
        public async Task<IEnumerable<FileModel>> Get(string path)
        {
            using (var connection = _factory())
            {
                await connection.ConnectAsync();
                var files = await connection.GetListingAsync(path);
                await connection.DisconnectAsync();

                return
                    from file in files
                    let folder = file.Type == FtpFileSystemObjectType.Directory
                    let size = ByteConverter.BytesToString(file.Size)
                    select new FileModel(file.Name, file.FullName, folder, size);
            }
        }

        public async Task CreateFolder(string path, string name)
        {
            using (var connection = _factory())
            {
                await connection.ConnectAsync();
                var directory = Path.Combine(path, name);
                await connection.CreateDirectoryAsync(directory);
                await connection.DisconnectAsync();
            }
        }

        public async Task RenameFile(FileModel file, string name)
        {
            using (var connection = _factory())
            {
                await connection.ConnectAsync();
                var directoryName = Path.GetDirectoryName(file.Path);
                var newName = Path.Combine(directoryName, name);
                await connection.RenameAsync(file.Path, newName);
                await connection.DisconnectAsync();
            }
        }

        public async Task Delete(FileModel file)
        {
            var path = file.Path;
            using (var connection = _factory())
            {
                await connection.ConnectAsync();
                if (file.IsFolder) await connection.DeleteDirectoryAsync(path);
                else await connection.DeleteFileAsync(path);
                await connection.DisconnectAsync();
            }
        }

        public Task Logout()
        {
            _factory = null;
            _isAuthorized.OnNext(false);
            return Task.CompletedTask;
        }

        public async Task UploadFile(string to, Stream from, string name)
        {
            using (var connection = _factory())
            {
                await connection.ConnectAsync();
                var path = Path.Combine(to, name);
                await connection.UploadAsync(from, path);
                await connection.DisconnectAsync();
            }
        }

        public async Task DownloadFile(string from, Stream to)
        {
            using (var connection = _factory())
            {
                await connection.ConnectAsync();
                await connection.DownloadAsync(to, from);
                await connection.DisconnectAsync();
            }
        }
    }
}