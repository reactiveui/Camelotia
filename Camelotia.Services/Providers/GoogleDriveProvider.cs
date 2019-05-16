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
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using File = Google.Apis.Drive.v3.Data.File;

namespace Camelotia.Services.Providers
{
    public sealed class GoogleDriveProvider : IProvider
    {
        private const string GoogleDriveApplicationName = "Camelotia";
        private const string GoogleDriveClientId = "1096201018044-qbv35mo5cd7b5utfjpg83v5lsuhssvvg.apps.googleusercontent.com";
        private const string GoogleDriveClientSecret = "L-xoeULle07kb_jHleqMxWo2";
        private const string GoogleDriveUserName = "user";
        
        private readonly ISubject<bool> _isAuthorized = new ReplaySubject<bool>(1);
        private readonly IBlobCache _blobCache;
        private DriveService _driveService;
        
        public GoogleDriveProvider(Guid id, IBlobCache blobCache)
        {
            Id = id;
            _blobCache = blobCache;
            _isAuthorized.OnNext(false);
            EnsureLoggedInIfTokenSaved();
        }

        public Guid Id { get; }

        public long? Size { get; } = null;

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
            return response.Files.Select(file => new FileModel
            {
                IsFolder = false,
                Size = file.Size.GetValueOrDefault(),
                Modified = file.ModifiedTime,
                Name = file.Name,
                Path = file.Id
            });
        }

        public async Task UploadFile(string to, Stream from, string name)
        {
            var create = _driveService.Files.Create(new File {Name = name}, from, "application/vnd.google-apps.file");
            await create.UploadAsync().ConfigureAwait(false);
        }

        public async Task DownloadFile(string from, Stream to)
        {
            var file = _driveService.Files.Get(from);
            var progress = await file.DownloadAsync(to).ConfigureAwait(false);
            while (progress.Status == DownloadStatus.Downloading)
                await Task.Delay(1000);
        }

        public async Task RenameFile(string path, string name)
        {
            var update = _driveService.Files.Update(new File {Name = name}, path);
            await update.ExecuteAsync().ConfigureAwait(false);
        }

        public async Task Delete(string path, bool isFolder)
        {
            var delete = _driveService.Files.Delete(path);
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
                var driveKeys = await _blobCache.GetAllKeys();
                if (driveKeys.Any(x => x.StartsWith($"google-drive-{Id}")))
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
                    new AkavacheDataStore(_blobCache, Id))
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
            private readonly Guid _id;

            public AkavacheDataStore(IBlobCache blobCache, Guid id)
            {
                _blobCache = blobCache;
                _id = id;
            }

            public async Task StoreAsync<T>(string key, T value)
            {
                var identity = $"google-drive-{_id}-{key}";
                await _blobCache.InsertObject(identity, value);
            }

            public async Task DeleteAsync<T>(string key)
            {
                var identity = $"google-drive-{_id}-{key}";
                await _blobCache.Invalidate(identity);
            }

            public async Task<T> GetAsync<T>(string key)
            {
                var identity = $"google-drive-{_id}-{key}";
                var value = await _blobCache.GetOrFetchObject(identity, () => Task.FromResult(default(T)));
                return value;
            }

            public Task ClearAsync() => Task.CompletedTask;
        }
    }
}