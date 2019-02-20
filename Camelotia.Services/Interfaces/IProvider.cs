using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Camelotia.Services.Models;

namespace Camelotia.Services.Interfaces
{
    public interface IProvider
    {
        string Size { get; }
        
        string Name { get; }
        
        string Description { get; }
        
        string InitialPath { get; }

        Task<IEnumerable<FileModel>> Get(string path);
        
        Task UploadFile(string to, Stream from, string name);

        Task DownloadFile(string from, Stream to);

        Task CreateFolder(string path, string name);

        Task RenameFile(FileModel file, string name);

        Task Delete(FileModel file);
        
        IObservable<bool> IsAuthorized { get; }
        
        bool SupportsDirectAuth { get; }
        
        bool SupportsHostAuth { get; }
        
        bool SupportsOAuth { get; }
        
        bool CanCreateFolder { get; }

        Task HostAuth(string address, int port, string login, string password);

        Task DirectAuth(string login, string password);

        Task OAuth();

        Task Logout();
    }
}