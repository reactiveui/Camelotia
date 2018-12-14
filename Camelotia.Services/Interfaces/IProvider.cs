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

        Task<IEnumerable<FileModel>> Get(string path);
        
        Task UploadFile(string to, Stream from, string name);

        Task DownloadFile(string from, Stream to);
        
        IObservable<bool> IsAuthorized { get; }
        
        bool SupportsDirectAuth { get; }
        
        bool SupportsOAuth { get; }

        Task DirectAuth(string login, string password);

        Task OAuth();

        Task Logout();
    }
}