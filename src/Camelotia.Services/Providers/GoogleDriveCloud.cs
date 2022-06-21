using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Akavache;
using Camelotia.Services.Configuration;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using File = Google.Apis.Drive.v3.Data.File;

namespace Camelotia.Services.Providers;

public sealed class GoogleDriveCloud : ICloud, IDisposable
{
    private readonly ReplaySubject<bool> _isAuthorized = new(1);
    private readonly GoogleDriveCloudOptions _options;
    private readonly IBlobCache _blobCache;
    private DriveService _driveService;

    public GoogleDriveCloud(CloudParameters model, IBlobCache blobCache, GoogleDriveCloudOptions options)
    {
        Parameters = model;
        _blobCache = blobCache;
        _options = options;
        _isAuthorized.OnNext(false);
        EnsureLoggedInIfTokenSaved();
    }

    public CloudParameters Parameters { get; }

    public long? Size => null;

    public Guid Id => Parameters.Id;

    public string Name => Parameters.Type.ToString();

    public DateTime Created => Parameters.Created;

    public string InitialPath => "/";

    public IObservable<bool> IsAuthorized => _isAuthorized;

    public bool SupportsDirectAuth => false;

    public bool SupportsHostAuth => false;

    public bool SupportsOAuth => true;

    public bool CanCreateFolder => false;

    public async Task<IEnumerable<FileModel>> GetFiles(string path)
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

    public Task<IEnumerable<FolderModel>> GetBreadCrumbs(string path) => throw new NotImplementedException();

    public async Task UploadFile(string to, Stream from, string name)
    {
        var create = _driveService.Files.Create(new File { Name = name }, from, "application/vnd.google-apps.file");
        await create.UploadAsync().ConfigureAwait(false);
    }

    public async Task DownloadFile(string from, Stream to)
    {
        var file = _driveService.Files.Get(from);
        var progress = await file.DownloadAsync(to).ConfigureAwait(false);
        while (progress.Status == DownloadStatus.Downloading)
            await Task.Delay(1000).ConfigureAwait(false);
    }

    public async Task RenameFile(string path, string name)
    {
        var update = _driveService.Files.Update(new File { Name = name }, path);
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
        foreach (var driveKey in keys.Where(x => x.StartsWith("google-drive", StringComparison.OrdinalIgnoreCase)))
            await _blobCache.Invalidate(driveKey);

        _driveService = null;
        _isAuthorized.OnNext(false);
        return Task.CompletedTask;
    });

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1075:Avoid empty catch clause that catches System.Exception.", Justification = "Deliberate ignore")]
    private void EnsureLoggedInIfTokenSaved() => Task.Run(async () =>
    {
        try
        {
            var driveKeys = await _blobCache.GetAllKeys();
            if (driveKeys.Any(x => x.StartsWith($"google-drive-{Id}", StringComparison.OrdinalIgnoreCase)))
                await AuthenticateAsync().ConfigureAwait(false);
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
                new ClientSecrets
                {
                    ClientId = _options.GoogleDriveClientId,
                    ClientSecret = _options.GoogleDriveClientSecret
                },
                new[] { DriveService.Scope.Drive },
                _options.GoogleDriveUserName,
                CancellationToken.None,
                new AkavacheDataStore(_blobCache, Id))
            .ConfigureAwait(false);

        var initializer = new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _options.GoogleDriveApplicationName
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
            return await _blobCache.GetOrFetchObject(identity, () => Task.FromResult(default(T)));
        }

        public Task ClearAsync() => Task.CompletedTask;
    }

    public void Dispose()
    {
        _driveService?.Dispose();
        _isAuthorized.Dispose();
    }
}
