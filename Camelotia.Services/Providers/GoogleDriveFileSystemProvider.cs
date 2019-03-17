using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Akavache;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using File = Google.Apis.Drive.v3.Data.File;

namespace Camelotia.Services.Providers
{
    public sealed class GoogleDriveFileSystemProvider : IProvider
    {
        private const string GoogleDriveApplicationName = "Camelotia";
        private const string GoogleDriveClientId = "1096201018044-qbv35mo5cd7b5utfjpg83v5lsuhssvvg.apps.googleusercontent.com";
        private const string GoogleDriveClientSecret = "w4F099v9awUEAs66rmCxLbYr";
        private const string GoogleDriveUserName = "user";
        
        private readonly ISubject<bool> _isAuthorized = new ReplaySubject<bool>(1);
        private readonly IBlobCache _blobCache;
        private DriveService _driveService;
        
        public GoogleDriveFileSystemProvider(Guid id, IBlobCache blobCache)
        {
            Id = id;
            _blobCache = blobCache;
            _isAuthorized.OnNext(false);
            EnsureLoggedInIfTokenSaved();
        }

        public Guid Id { get; }

        public string Size { get; } = "Unknown";

        public string Name { get; } = "Google Drive";

        public string Description { get; } = "Google Drive file system.";

        public string InitialPath { get; } = "/";

        public IObservable<bool> IsAuthorized => _isAuthorized;

        public bool SupportsDirectAuth => false;
        
        public bool SupportsHostAuth => false;
        
        public bool SupportsOAuth => true;
        
        public bool CanCreateFolder => false;
        
        public async Task<IEnumerable<FileModel>> Get(string path)
        {
            var list = _driveService.Files.List();
            list.PageSize = 1000;
            list.Fields = "files(id, name, size, modifiedTime)";
            
            var response = await list.ExecuteAsync().ConfigureAwait(false);
            var files = from file in response.Files
                        let size = file.Size.GetValueOrDefault()
                        let bytes = ByteConverter.BytesToString(size)
                        select new FileModel(file.Name, file.Id, false, bytes, file.ModifiedTime);

            return files;
        }

        public Task UploadFile(string to, Stream from, string name) => throw new NotImplementedException();

        public Task DownloadFile(string from, Stream to) => throw new NotImplementedException();

        public async Task RenameFile(FileModel file, string name)
        {
            var update = _driveService.Files.Update(new File {Name = name}, file.Path);
            await update.ExecuteAsync().ConfigureAwait(false);
        }

        public async Task Delete(FileModel file)
        {
            var delete = _driveService.Files.Delete(file.Path);
            await delete.ExecuteAsync().ConfigureAwait(false);
        }

        public Task CreateFolder(string path, string name) => Task.CompletedTask;

        public Task HostAuth(string address, int port, string login, string password) => Task.CompletedTask;

        public Task DirectAuth(string login, string password) => Task.CompletedTask;

        public Task OAuth() => Task.Run(AuthenticateAsync);

        public Task Logout() => Task.Run(async () =>
        {
            var keys = await _blobCache.GetAllKeys();
            var googleDriveKeys = keys.Where(x => x.StartsWith("google-drive"));
            foreach (var driveKey in googleDriveKeys) 
                await _blobCache.Invalidate(driveKey);

            _driveService = null;
            _isAuthorized.OnNext(false);
            return Task.CompletedTask;
        });

        private void EnsureLoggedInIfTokenSaved() => Task.Run(async () =>
        {
            try
            {
                await AuthenticateAsync();
            }
            catch (Exception)
            {
                // ignore
            }
        });

        private async Task AuthenticateAsync()
        {
            var credential = await GoogleWebAuthorizationBroker
                .AuthorizeAsync(
                    new ClientSecrets {ClientId = GoogleDriveClientId, ClientSecret = GoogleDriveClientSecret},
                    new[] {DriveService.Scope.Drive},
                    GoogleDriveUserName,
                    CancellationToken.None,
                    new AkavacheDataStore(_blobCache))
                .ConfigureAwait(false);

            var initializer = new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = GoogleDriveApplicationName
            };

            _driveService = new DriveService(initializer);
            _isAuthorized.OnNext(true);
        }

        private sealed class AkavacheDataStore : IDataStore
        {
            private readonly IBlobCache _blobCache;

            public AkavacheDataStore(IBlobCache blobCache) => _blobCache = blobCache;

            public async Task StoreAsync<T>(string key, T value)
            {
                var identity = $"google-drive-{key}";
                await _blobCache.InsertObject(identity, value);
            }

            public async Task DeleteAsync<T>(string key)
            {
                var identity = $"google-drive-{key}";
                await _blobCache.Invalidate(identity);
            }

            public async Task<T> GetAsync<T>(string key)
            {
                var identity = $"google-drive-{key}";
                var value = await _blobCache.GetOrFetchObject<T>(identity, () => Task.FromResult(default(T)));
                return value;
            }

            public Task ClearAsync() => Task.CompletedTask;
        }
    }
}