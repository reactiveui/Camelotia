// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using Akavache;
using Camelotia.Services.Configuration;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Signals;
using File = Google.Apis.Drive.v3.Data.File;

namespace Camelotia.Services.Providers;

/// <summary>Browses the files in the user's Google Drive.</summary>
[System.Diagnostics.DebuggerDisplay("GoogleDriveCloud: {Parameters}")]
public sealed class GoogleDriveCloud : ICloud, IDisposable
{
    /// <summary>The most files one list request returns; 1000 is the Drive API maximum.</summary>
    private const int PageSize = 1000;

    /// <summary>The cache key prefix every Google Drive credential uses.</summary>
    private const string KeyPrefix = "google-drive-";

    /// <summary>Replays whether the user is signed in.</summary>
    private readonly ReplaySignal<bool> _isAuthorized = new(1);

    /// <summary>Cancels the background sign-in when the provider is disposed.</summary>
    private readonly CancellationTokenSource _lifetime = new();

    /// <summary>The Google application settings.</summary>
    private readonly GoogleDriveCloudOptions _options;

    /// <summary>The cache that stores the Google credential.</summary>
    private readonly IBlobCache _blobCache;

    /// <summary>Signs the user in to Google.</summary>
    private readonly GoogleDriveAuthorizer _authorizer;

    /// <summary>Creates the HTTP clients Drive requests use, or <see langword="null"/> for Google's default.</summary>
    private readonly Google.Apis.Http.IHttpClientFactory? _httpClientFactory;

    /// <summary>The Drive client, or <see langword="null"/> when signed out.</summary>
    private DriveService? _driveService;

    /// <summary>Initializes a new instance of the <see cref="GoogleDriveCloud"/> class that signs in through the browser.</summary>
    /// <param name="model">The saved parameters.</param>
    /// <param name="blobCache">The cache that stores the Google credential.</param>
    /// <param name="options">The Google application settings.</param>
    public GoogleDriveCloud(CloudParameters model, IBlobCache blobCache, GoogleDriveCloudOptions options)
        : this(model, blobCache, options, AuthorizeInBrowser, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="GoogleDriveCloud"/> class.</summary>
    /// <param name="model">The saved parameters.</param>
    /// <param name="blobCache">The cache that stores the Google credential.</param>
    /// <param name="options">The Google application settings.</param>
    /// <param name="authorizer">Signs the user in to Google.</param>
    /// <param name="httpClientFactory">Creates the HTTP clients Drive requests use, or <see langword="null"/> for Google's default.</param>
    public GoogleDriveCloud(
        CloudParameters model,
        IBlobCache blobCache,
        GoogleDriveCloudOptions options,
        GoogleDriveAuthorizer authorizer,
        Google.Apis.Http.IHttpClientFactory? httpClientFactory)
    {
        Parameters = model;
        _blobCache = blobCache;
        _options = options;
        _authorizer = authorizer;
        _httpClientFactory = httpClientFactory;
        _isAuthorized.OnNext(false);
        _ = RestoreSignIn(_lifetime.Token);
    }

    /// <inheritdoc/>
    public CloudParameters Parameters { get; }

    /// <inheritdoc/>
    public long? Size => null;

    /// <inheritdoc/>
    public Guid Id => Parameters.Id;

    /// <inheritdoc/>
    public string Name => Parameters.Type.ToString();

    /// <inheritdoc/>
    public DateTimeOffset Created => Parameters.Created;

    /// <inheritdoc/>
    public string InitialPath => "/";

    /// <inheritdoc/>
    public IObservable<bool> IsAuthorized => _isAuthorized;

    /// <inheritdoc/>
    public bool SupportsDirectAuth => false;

    /// <inheritdoc/>
    public bool SupportsHostAuth => false;

    /// <inheritdoc/>
    public bool SupportsOAuth => true;

    /// <inheritdoc/>
    public bool CanCreateFolder => false;

    /// <summary>Gets the cache key prefix for this provider instance's credential.</summary>
    private string OwnedKeyPrefix => $"{KeyPrefix}{Id}-";

    /// <inheritdoc/>
    public async ValueTask<IEnumerable<FileModel>> GetFiles(string path, CancellationToken cancellationToken)
    {
        var drive = RequireDrive();
        var files = new List<FileModel>();
        string? pageToken = null;
        do
        {
            var request = drive.Files.List();
            request.PageSize = PageSize;
            request.PageToken = pageToken;
            request.Q = "trashed = false and mimeType != 'application/vnd.google-apps.folder'";
            request.Fields = "nextPageToken, files(id, name, size, modifiedTime)";
            var page = await request.ExecuteAsync(cancellationToken).ConfigureAwait(false);
            foreach (var file in page.Files)
            {
                files.Add(new() { IsFolder = false, Size = file.Size.GetValueOrDefault(), Modified = file.ModifiedTimeDateTimeOffset, Name = file.Name, Path = file.Id });
            }

            pageToken = page.NextPageToken;
        }
        while (pageToken is not null);

        return files;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<IEnumerable<FolderModel>> GetBreadCrumbs(string path, CancellationToken cancellationToken) =>
        ValueTask.FromResult<IEnumerable<FolderModel>>([]);

    /// <inheritdoc/>
    public async ValueTask UploadFile(string toPath, Stream fromStream, string name, CancellationToken cancellationToken)
    {
        var upload = RequireDrive().Files.Create(new File { Name = name }, fromStream, "application/octet-stream");
        var progress = await upload.UploadAsync(cancellationToken).ConfigureAwait(false);
        if (progress.Status != UploadStatus.Completed)
        {
            throw new IOException($"Google Drive did not accept {name}.", progress.Exception);
        }
    }

    /// <inheritdoc/>
    public async ValueTask DownloadFile(string fromPath, Stream fromStream, CancellationToken cancellationToken)
    {
        var progress = await RequireDrive().Files.Get(fromPath).DownloadAsync(fromStream, cancellationToken).ConfigureAwait(false);
        if (progress.Status != DownloadStatus.Completed)
        {
            throw new IOException("Google Drive could not send the file.", progress.Exception);
        }
    }

    /// <inheritdoc/>
    public async ValueTask RenameFile(string path, string name, CancellationToken cancellationToken) =>
        _ = await RequireDrive().Files.Update(new File { Name = name }, path).ExecuteAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async ValueTask Delete(string path, bool isFolder, CancellationToken cancellationToken) =>
        _ = await RequireDrive().Files.Delete(path).ExecuteAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public ValueTask CreateFolder(string path, string name, CancellationToken cancellationToken) =>
        throw new NotSupportedException("Camelotia shows Google Drive as one flat list of files.");

    /// <inheritdoc/>
    public ValueTask HostAuth(string address, int port, string login, string password, CancellationToken cancellationToken) =>
        throw new NotSupportedException("Google Drive signs in through the browser.");

    /// <inheritdoc/>
    public ValueTask DirectAuth(string login, string password, CancellationToken cancellationToken) =>
        throw new NotSupportedException("Google Drive signs in through the browser.");

    /// <inheritdoc/>
    public async ValueTask OAuth(CancellationToken cancellationToken)
    {
        var secrets = new ClientSecrets { ClientId = _options.GoogleDriveClientId, ClientSecret = _options.GoogleDriveClientSecret };
        var credential = await _authorizer(secrets, _options.GoogleDriveUserName, new AkavacheDataStore(_blobCache, OwnedKeyPrefix), cancellationToken).ConfigureAwait(false);

        var initializer = new BaseClientService.Initializer { HttpClientInitializer = credential, ApplicationName = _options.GoogleDriveApplicationName };
        if (_httpClientFactory is not null)
        {
            initializer.HttpClientFactory = _httpClientFactory;
        }

        _driveService?.Dispose();
        _driveService = new(initializer);
        _isAuthorized.OnNext(true);
    }

    /// <inheritdoc/>
    public async ValueTask Logout(CancellationToken cancellationToken)
    {
        var owned = await GetOwnedKeys(cancellationToken).ConfigureAwait(false);
        _ = await _blobCache.Invalidate(owned).ToTask(cancellationToken).ConfigureAwait(false);
        _driveService?.Dispose();
        _driveService = null;
        _isAuthorized.OnNext(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _lifetime.Cancel();
        _lifetime.Dispose();
        _driveService?.Dispose();
        _isAuthorized.Dispose();
    }

    /// <summary>Signs in through Google's browser flow, which listens on a loopback address for the redirect.</summary>
    /// <param name="secrets">The OAuth client identifier and secret.</param>
    /// <param name="user">The local user name the credential is stored under.</param>
    /// <param name="store">The store that keeps the credential between runs.</param>
    /// <param name="cancellationToken">Cancels the sign-in.</param>
    /// <returns>The credential.</returns>
    private static async Task<IConfigurableHttpClientInitializer> AuthorizeInBrowser(ClientSecrets secrets, string user, IDataStore store, CancellationToken cancellationToken) =>
        await GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, [DriveService.Scope.Drive], user, cancellationToken, store).ConfigureAwait(false);

    /// <summary>Lists the cache keys that hold this provider's credential.</summary>
    /// <param name="cancellationToken">Cancels the lookup.</param>
    /// <returns>The keys.</returns>
    private async ValueTask<List<string>> GetOwnedKeys(CancellationToken cancellationToken)
    {
        var keys = await _blobCache.GetAllKeys().ToList().ToTask(cancellationToken).ConfigureAwait(false);
        var owned = new List<string>();
        foreach (var key in keys)
        {
            if (key.StartsWith(OwnedKeyPrefix, StringComparison.Ordinal))
            {
                owned.Add(key);
            }
        }

        return owned;
    }

    /// <summary>Signs the user back in from a saved credential without opening the browser.</summary>
    /// <param name="cancellationToken">Cancels the sign-in; <see cref="Dispose"/> cancels it.</param>
    /// <returns>A task that completes when the attempt ends. A credential that no longer works leaves the user signed out.</returns>
    private async Task RestoreSignIn(CancellationToken cancellationToken)
    {
        try
        {
            if ((await GetOwnedKeys(cancellationToken).ConfigureAwait(false)).Count > 0)
            {
                await OAuth(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception exception) when (exception is TokenResponseException or HttpRequestException or OperationCanceledException or KeyNotFoundException or ObjectDisposedException)
        {
            // Dispose cancels the token before it releases the signal, so a disposed provider publishes nothing.
            if (!cancellationToken.IsCancellationRequested)
            {
                _isAuthorized.OnNext(false);
            }
        }
    }

    /// <summary>Gets the Drive client for the signed-in user.</summary>
    /// <returns>The client.</returns>
    /// <exception cref="InvalidOperationException">No user is signed in.</exception>
    private DriveService RequireDrive() => _driveService ?? throw new InvalidOperationException("Sign in to Google Drive first.");

    /// <summary>Stores the Google credential in Akavache under keys that start with a per-provider prefix.</summary>
    /// <param name="blobCache">The cache.</param>
    /// <param name="prefix">The key prefix for this provider.</param>
    private sealed class AkavacheDataStore(IBlobCache blobCache, string prefix) : IDataStore
    {
        /// <summary>Saves a value.</summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="key">The Google key.</param>
        /// <param name="value">The value to save.</param>
        /// <returns>A task that completes when the value is saved.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task StoreAsync<T>(string key, T value) => blobCache.InsertObject(prefix + key, value).ToTask();

        /// <summary>Removes a value.</summary>
        /// <typeparam name="T">The value type, which Akavache stores typed entries under.</typeparam>
        /// <param name="key">The Google key.</param>
        /// <returns>A task that completes when the value is gone.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task DeleteAsync<T>(string key) => blobCache.Invalidate(prefix + key, typeof(T)).ToTask();

        /// <summary>Reads a value.</summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="key">The Google key.</param>
        /// <returns>The value, or the default value when none is saved, as Google's <see cref="IDataStore"/> contract expects.</returns>
        public async Task<T> GetAsync<T>(string key)
        {
            var keys = await blobCache.GetAllKeys().ToList().ToTask().ConfigureAwait(false);
            if (!keys.Contains(prefix + key))
            {
                return default!;
            }

            var value = await blobCache.GetObject<T>(prefix + key).ToTask().ConfigureAwait(false);
            return value!;
        }

        /// <summary>Does nothing; <see cref="Logout"/> removes this provider's keys.</summary>
        /// <returns>A completed task.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task ClearAsync() => Task.CompletedTask;
    }
}
