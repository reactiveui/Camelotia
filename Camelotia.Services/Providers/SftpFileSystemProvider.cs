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
    public sealed class SftpFileSystemProvider : IProvider
    {
        private readonly ISubject<bool> _isAuthorized = new ReplaySubject<bool>();
        private Func<SftpClient> _factory;

        public SftpFileSystemProvider() => _isAuthorized.OnNext(false);
        
        public string Size => "Unknown";
        
        public string Name => "SFTP";
        
        public string Description => "Secure FTP file provider.";

        public string InitialPath => Path.DirectorySeparatorChar.ToString();

        public IObservable<bool> IsAuthorized => _isAuthorized;
        
        public bool SupportsDirectAuth => false;
        
        public bool SupportsHostAuth => true;
        
        public bool SupportsOAuth => false;

        public bool CanCreateFolder => false;

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
            using (var connection = _factory())
            {
                connection.Connect();
                var contents = connection.ListDirectory(path);
                connection.Disconnect();

                return
                    from file in contents
                    let size = ByteConverter.BytesToString(file.Length)
                    where file.Name != "." && file.Name != ".."
                    select new FileModel(file.Name, file.FullName, file.IsDirectory, size);
            }
        });

        public Task CreateFolder(string path, string name)
        {
            throw new NotImplementedException();
        }

        public Task Delete(FileModel file) => Task.Run(() =>
        {
            using (var connection = _factory())
            {
                connection.Connect();
                connection.DeleteFile(file.Path);
                connection.Disconnect();
            }
        });

        public Task UploadFile(string to, Stream from, string name) => Task.Run(() =>
        {
            using (var connection = _factory())
            {
                connection.Connect();
                var path = Path.Combine(to, name);
                connection.UploadFile(from, path);
                connection.Disconnect();
            }
        });

        public Task DownloadFile(string from, Stream to) => Task.Run(() =>
        {
            using (var connection = _factory())
            {
                connection.Connect();
                connection.DownloadFile(from, to);
                connection.Disconnect();
            }
        });
    }
}