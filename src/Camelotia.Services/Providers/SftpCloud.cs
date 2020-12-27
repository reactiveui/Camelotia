using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Renci.SshNet;

namespace Camelotia.Services.Providers
{
    public sealed class SftpCloud : ICloud
    {
        private static readonly string[] PathSeparators = { "\\", "/" };

        private readonly ISubject<bool> _isAuthorized = new ReplaySubject<bool>();
        private Func<SftpClient> _factory;

        public SftpCloud(CloudParameters model)
        {
            Parameters = model;
            _isAuthorized.OnNext(false);
        }

        public CloudParameters Parameters { get; }

        public long? Size => null;

        public Guid Id => Parameters.Id;

        public string Name => Parameters.Type.ToString();

        public DateTime Created => Parameters.Created;
        
        public string InitialPath => Path.DirectorySeparatorChar.ToString();

        public IObservable<bool> IsAuthorized => _isAuthorized;
        
        public bool SupportsDirectAuth => false;
        
        public bool SupportsHostAuth => true;
        
        public bool SupportsOAuth => false;

        public bool CanCreateFolder => true;

        public Task DirectAuth(string login, string password) => Task.CompletedTask;

        public Task OAuth() => Task.CompletedTask;

        public Task HostAuth(string address, int port, string login, string password) => Task.Run(() =>
        {
            _factory = () =>
            {
                var sftp = new SftpClient(address, port, login, password);
                sftp.HostKeyReceived += (sender, args) => args.CanTrust = true;
                return sftp;
            };
            using (var connection = _factory())
            {
                connection.Connect();
                connection.ListDirectory("/");
                connection.Disconnect();
            }
            _isAuthorized.OnNext(true);
        });
        
        public Task Logout()
        {
            _factory = null;
            _isAuthorized.OnNext(false);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<FileModel>> Get(string path) => Task.Run(() =>
        {
            path = path.Replace("\\", "/");
            using var connection = _factory();
            connection.Connect();
            var contents = connection.ListDirectory(path);
            connection.Disconnect();
            return contents
                .Where(file => file.Name != "." && file.Name != "..")
                .Select(file => new FileModel
                {
                    Name = file.Name,
                    Path = file.FullName,
                    IsFolder = file.IsDirectory,
                    Modified = file.LastWriteTime,
                    Size = file.Length
                })
                .ToList()
                .AsEnumerable();
        });

        public Task<IEnumerable<FolderModel>> GetBreadCrumbs(string path) => Task.Run(() =>
        {
            var pathParts = new List<string> { "/" }; //Add root path first
            pathParts.AddRange(path.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries));
            var foldermodels = new List<FolderModel>();
            using var connection = _factory();
            connection.Connect();
            for (var i = 0; i < pathParts.Count; i++)
            {
                var fullPath = string.Join(PathSeparators[0], pathParts.Take(i + 1));
                var name = pathParts[i];
                var listing = connection.ListDirectory(fullPath);
                var folder = new FolderModel(
                    fullPath,
                    name,
                    listing
                        .Where(f => f.IsDirectory && f.Name != "." && f.Name != "..")
                        .Select(f => new FolderModel(f.FullName, f.Name)));
                foldermodels.Add(folder);
            }
            connection.Disconnect();
            return foldermodels.AsEnumerable();
        });

        public Task CreateFolder(string path, string name) => Task.Run(() =>
        {
            using var connection = _factory();
            connection.Connect();
            var directory = Path.Combine(path, name);
            connection.CreateDirectory(directory);
            connection.Disconnect();
        });

        public Task RenameFile(string path, string name) => Task.Run(() =>
        {
            using var connection = _factory();
            connection.Connect();
            var directoryName = Path.GetDirectoryName(path);
            var newName = Path.Combine(directoryName, name);
            connection.RenameFile(path, newName);
            connection.Disconnect();
        });

        public Task Delete(string path, bool isFolder) => Task.Run(() =>
        {
            using var connection = _factory();
            connection.Connect();
            connection.DeleteFile(path);
            connection.Disconnect();
        });

        public Task UploadFile(string to, Stream from, string name) => Task.Run(() =>
        {
            using var connection = _factory();
            connection.Connect();
            var path = Path.Combine(to, name);
            connection.UploadFile(@from, path);
            connection.Disconnect();
        });

        public Task DownloadFile(string from, Stream to) => Task.Run(() =>
        {
            using var connection = _factory();
            connection.Connect();
            connection.DownloadFile(@from, to);
            connection.Disconnect();
        });
    }
}