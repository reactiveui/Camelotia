using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;

namespace Camelotia.Services.Providers
{
    public sealed class FtpFileSystemProvider : IProvider
    {
        private readonly ISubject<bool> _isAuthorized = new ReplaySubject<bool>();

        public FtpFileSystemProvider() => _isAuthorized.OnNext(false);
        
        public string Size => "Unknown";

        public string Name => "FTP";

        public string InitialPath => "/";

        public string Description => "FTP remote file system.";

        public IObservable<bool> IsAuthorized => _isAuthorized;

        public bool SupportsDirectAuth => false;

        public bool SupportsHostAuth => true;

        public bool SupportsOAuth => false;

        public Task DirectAuth(string login, string password) => Task.CompletedTask;

        public Task OAuth() => Task.CompletedTask;
        
        public Task HostAuth(string address, string login, string password)
        {
            throw new NotImplementedException();
        }
        
        public Task<IEnumerable<FileModel>> Get(string path)
        {
            throw new NotImplementedException();
        }

        public Task UploadFile(string to, Stream from, string name)
        {
            throw new NotImplementedException();
        }

        public Task DownloadFile(string from, Stream to)
        {
            throw new NotImplementedException();
        }

        public Task Delete(FileModel file)
        {
            throw new NotImplementedException();
        }

        public Task Logout()
        {
            throw new NotImplementedException();
        }
    }
}