using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Camelotia.Services.Models;

namespace Camelotia.Services.Interfaces
{
    public interface IProvider
    {
        Guid Id { get; }
        
        long? Size { get; }
        
        string Name { get; }
        
        DateTime Created { get; }
        
        string InitialPath { get; }

        Task<IEnumerable<FileModel>> Get(string path);
        
        Task UploadFile(string to, Stream from, string name);

        Task DownloadFile(string from, Stream to);

        bool CanCreateFolder { get; }

        Task CreateFolder(string path, string name);

        Task RenameFile(string path, string name);

        Task Delete(string path, bool isFolder);
        
        IObservable<bool> IsAuthorized { get; }
        
        bool SupportsDirectAuth { get; }

        Task DirectAuth(string login, string password);

        bool SupportsHostAuth { get; }

        Task HostAuth(string address, int port, string login, string password);

        bool SupportsOAuth { get; }

        Task OAuth();

        Task Logout();
    }
}