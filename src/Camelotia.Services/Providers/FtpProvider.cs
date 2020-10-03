using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using FluentFTP;

namespace Camelotia.Services.Providers
{
    public sealed class FtpProvider : IProvider
    {
        private static readonly string[] PathSeparators = { "\\", "/" };
        
        private readonly ISubject<bool> _isAuthorized = new ReplaySubject<bool>();
        private Func<FtpClient> _factory;

        public FtpProvider(ProviderParameters model)
        {
            Parameters = model;
            _isAuthorized.OnNext(false);
        }

        public ProviderParameters Parameters { get; }

        public long? Size => null;

        public Guid Id => Parameters.Id;

        public string InitialPath => "/";

        public string Name => Parameters.Type.ToString();

        public DateTime Created => Parameters.Created;

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
            using var connection = _factory();
            await connection.ConnectAsync();
            var files = await connection.GetListingAsync(path);
            await connection.DisconnectAsync();
            return files.Select(file => new FileModel
            {
                IsFolder = file.Type == FtpFileSystemObjectType.Directory,
                Modified = file.Modified,
                Name = file.Name,
                Path = file.FullName,
                Size = file.Size
            });
        }

        public async Task<IEnumerable<FolderModel>> GetBreadCrumbs(string path)
        {
            var pathParts = new List<string> { "/" }; //Add root path first
            pathParts.AddRange(path.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries));
            var foldermodels = new List<FolderModel>();
            using var connection = _factory();
            await connection.ConnectAsync();
            for (var i = 0; i < pathParts.Count; i++)
            {
                var fullPath = string.Join(PathSeparators[0], pathParts.Take(i+1));                    
                var name = pathParts[i];
                var listing = await connection.GetListingAsync(fullPath);
                var folder = new FolderModel(
                    fullPath,
                    name,
                    listing
                        .Where(f => f.Type == FtpFileSystemObjectType.Directory)
                        .Select(f => new FolderModel(f.FullName, f.Name)));
                foldermodels.Add(folder);
            }
            await connection.DisconnectAsync();
            return foldermodels;
        }

        public async Task CreateFolder(string path, string name)
        {
            using var connection = _factory();
            await connection.ConnectAsync();
            var directory = Path.Combine(path, name);
            await connection.CreateDirectoryAsync(directory);
            await connection.DisconnectAsync();
        }

        public async Task RenameFile(string path, string name)
        {
            using var connection = _factory();
            await connection.ConnectAsync();
            var directoryName = Path.GetDirectoryName(path);
            var newName = Path.Combine(directoryName, name);
            await connection.RenameAsync(path, newName);
            await connection.DisconnectAsync();
        }

        public async Task Delete(string path, bool isFolder)
        {
            using var connection = _factory();
            await connection.ConnectAsync();
            if (isFolder) await connection.DeleteDirectoryAsync(path);
            else await connection.DeleteFileAsync(path);
            await connection.DisconnectAsync();
        }

        public Task Logout()
        {
            _factory = null;
            _isAuthorized.OnNext(false);
            return Task.CompletedTask;
        }

        public async Task UploadFile(string to, Stream from, string name)
        {
            using var connection = _factory();
            await connection.ConnectAsync();
            var path = Path.Combine(to, name);
            await connection.UploadAsync(@from, path);
            await connection.DisconnectAsync();
        }

        public async Task DownloadFile(string from, Stream to)
        {
            using var connection = _factory();
            await connection.ConnectAsync();
            await connection.DownloadAsync(to, @from);
            await connection.DisconnectAsync();
        }
    }
}